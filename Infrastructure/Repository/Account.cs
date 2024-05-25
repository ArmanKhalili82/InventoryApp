using Application.DTO.Request.Identity;
using Application.DTO.Response;
using Application.DTO.Response.Identity;
using Application.Extensions.Identity;
using Application.Interface.Identity;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Infrastructure.Repository
{
    public class Account: IAcount
        //(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : IAcount
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        public Account(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model)
        {
            var user = await FindUserByEmail(model.Email);
            if (user != null)
                return new ServiceResponse(false, "User Already Exist");

            var newUser = new ApplicationUser()
            {
                UserName = model.Email,
                PasswordHash = model.Password,
                Email = model.Email,
                Name = model.Name
            };
            var result = CheckResult(await _userManager.CreateAsync(newUser, model.Password));
            if (!result.Flag)
                return result;
            else
                return await CreateUserClaims(model);
        }

        private async Task<ServiceResponse> CreateUserClaims(CreateUserRequestDTO model)
        {
            if (string.IsNullOrEmpty(model.Policy)) return new ServiceResponse(false, "No Policy Specified");
            Claim[] userClaims = [];
            if (model.Policy.Equals(Policy.AdminPolicy, StringComparison.OrdinalIgnoreCase))
            {
                userClaims = [
                        new Claim(ClaimTypes.Email, model.Email),
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim("Name", model.Name),
                        new Claim("Create", "true"),
                        new Claim("Update", "true"),
                        new Claim("Delete", "true"),
                        new Claim("Read", "true"),
                        new Claim("ManageUser", "true")
                    ];
            }

            else if(model.Policy.Equals(Policy.ManagerPolicy, StringComparison.OrdinalIgnoreCase))
            {
                userClaims = [
                        new Claim(ClaimTypes.Email, model.Email),
                        new Claim(ClaimTypes.Role, "Manager"),
                        new Claim("Name", model.Name),
                        new Claim("Create", "true"),
                        new Claim("Update", "true"),
                        new Claim("Delete", "false"),
                        new Claim("Read", "true"),
                        new Claim("ManageUser", "false")
                    ];
            }

            else if (model.Policy.Equals(Policy.UserPolicy, StringComparison.OrdinalIgnoreCase))
            {
                userClaims = [
                        new Claim(ClaimTypes.Email, model.Email),
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim("Name", model.Name),
                        new Claim("Create", "false"),
                        new Claim("Update", "false"),
                        new Claim("Delete", "false"),
                        new Claim("Read", "false"),
                        new Claim("ManageUser", "false")
                    ];
            }

            var result = CheckResult(await _userManager.AddClaimAsync((await FindUserByEmail(model.Email)), userClaims));
            if (result.Flag)
                return new ServiceResponse(true, "User Created");
            else
                return result;

        }

        public async Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUserWithClaimsAsync()
        {
            var userList = new List<GetUserWithClaimResponseDTO>();
            var allUsers = await _userManager.Users.ToListAsync();
            if (allUsers.Count == 0) return userList;

            foreach (var user in allUsers)
            {
                var currentUser = await _userManager.FindByIdAsync(user.Id);
                var getCurrentUserClaims = await _userManager.GetClaimsAsync(currentUser);
                if (getCurrentUserClaims.Any())
                {
                    userList.Add(new GetUserWithClaimResponseDTO()
                    {
                        UserId = user.Id,
                        Email = getCurrentUserClaims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value,
                        RoleName = getCurrentUserClaims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value,
                        Name = getCurrentUserClaims.FirstOrDefault(x => x.Type == "Name").Value,
                        ManageUser = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(x => x.Type == "ManageUser").Value),
                        Create = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(x => x.Type == "Create").Value),
                        Update = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(x => x.Type == "Update").Value),
                        Delete = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(x => x.Type == "Delete").Value),
                        Read = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(x => x.Type == "Read").Value)
                    });
                }
            }
            return userList;
        }

        public async Task<ServiceResponse> LoginAsync(LoginUserRequestDTO model)
        {
            var user = await FindUserByEmail(model.Email);
            if (user is null) return new ServiceResponse(false, "User Not found");

            var verifyPassword = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!verifyPassword.Succeeded) return new ServiceResponse(false, "Incorect Credentials Provided");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
                return new ServiceResponse(false, "Unknown Error Occured While Logging You In");
            else
                return new ServiceResponse(true, null);
        }

        private async Task<ApplicationUser> FindUserByEmail(string email)
            => await _userManager.FindByEmailAsync(email);

        private async Task<ApplicationUser> FindUserById(string id) => await _userManager.FindByIdAsync(id);

        private static ServiceResponse CheckResult(IdentityResult result)
        {
            if (result.Succeeded) return new ServiceResponse(true, null);

            var errors = result.Errors.Select(x => x.Description);
            return new ServiceResponse(false, string.Join(Environment.NewLine, errors));
        }

        public async Task SetUpAsync() => await CreateUserAsync(new CreateUserRequestDTO()
        {
            Name = "Administrator",
            Email = "admin@admin.com",
            Password = "Admin@123",
            Policy = Policy.AdminPolicy
        });

        public async Task<ServiceResponse> UpdateUserAsync(ChangeUserClaimRequestDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return new ServiceResponse(false, "User Not Found");

            var oldUserClaims = await _userManager.GetClaimsAsync(user);
            Claim[] newUserClaims = [
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, model.RoleName),
                    new Claim("Name", model.Name),
                    new Claim("Create", model.Create.ToString()),
                    new Claim("Update", model.Update.ToString()),
                    new Claim("Read", model.Read.ToString()),
                    new Claim("ManageUser", model.ManageUser.ToString()),
                    new Claim("Delete", model.Delete.ToString())
                ];
            var result = await _userManager.RemoveClaimsAsync(user, oldUserClaims);
            var response = CheckResult(result);
            if (!response.Flag)
                return new ServiceResponse(false, response.Message);

            var addNewClaims = await _userManager.AddClaimsAsync(user, newUserClaims);
            var outcome = CheckResult(addNewClaims);
            if (outcome.Flag)
                return new ServiceResponse(true, "User Updated");
            else
                return outcome;
        }

        //public async Task SaveActivityAsync(ActivityTrackerResponseDTO model)
        //{
        //    context.ActivityTracker.Add(model.Adapt(new Tracker()));
        //    await context.SaveChangesAsync();
        //}

        //public async Task<IEnumerable<ActivityTrackerResponseDTO>> GetActivitiesAsync()
        //{
        //    var list = new List<ActivityTrackerResponseDTO>();
        //    var data = (await context.ActivityTracker.ToListAsync()).Adapt <List<ActivityTrackerResponseDTO>();
        //    foreach (var activity in data)
        //    {
        //        activity.UserName = (await FindUserById(activity.UserId)).Name;
        //        list.Add(activity);
        //    }

        //    return data;
        //}
    }
}
