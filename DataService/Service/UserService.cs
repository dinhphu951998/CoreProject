using CSharpProjectCore.Core.Constant;
using CSharpProjectCore.Core.Entities;
using CSharpProjectCore.Core.Models;
using CSharpProjectCore.Core.Utils;
using CSharpProjectCore.Core.ViewModels;
using CSharpProjectCore.Infrastructure;
using DataService.Repository;
using DataService.UoW;
using DataService.Validation;
using FirebaseAdmin.Auth;
using FluentValidation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Service
{
    public interface IUserService : IBaseService<User>
    {
        AccessTokenResponse Authenticate(UserAuthentication user);
        Task<AccessTokenResponse> Register(RegisteredUser user);
        Task<AccessTokenResponse> RegisterExternalUsingFirebaseAsync(FirebaseRegisterExternal external);
    }

    public class UserService : BaseService<User>, IUserService
    {
        private readonly JwtTokenProvider jwtTokenProvider;
        private readonly IUserRepository repository;

        public UserService(IUserRepository repository,
                            UnitOfWork unitOfWork,
                            JwtTokenProvider jwtTokenProvider) : base(unitOfWork)
        {
            this.jwtTokenProvider = jwtTokenProvider;
            this.repository = repository;
        }

        public AccessTokenResponse Authenticate(UserAuthentication userAuthen)
        {
            var user = repository.GetUserByUsername(userAuthen.Username);
            AccessTokenResponse token = null;

            UserAuthenticationValidation validation = new UserAuthenticationValidation();
            validation.Validate(userAuthen);

            if (user == null) throw new BaseException(ErrorMessage.CREDENTIALS_NOT_MATCH);

            var result = PasswordManipulation.VerifyPasswordHash(userAuthen.Password,
                                user.PasswordHash, user.PasswordSalt);
            if (result)
            {
                token = CreateToken(user);
            }
            else
            {
                throw new BaseException(ErrorMessage.CREDENTIALS_NOT_MATCH);
            }

            return token;
        }

        public async Task<AccessTokenResponse> Register(RegisteredUser userRegister)
        {
            RegisteredUserValidation validation = new RegisteredUserValidation(this.repository);
            validation.ValidateAndThrow(userRegister);

            var user = userRegister.ToEntity<User>();

            try
            {
                byte[] hash, salt;
                PasswordManipulation.CreatePasswordHash(userRegister.Password, out hash, out salt);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;

                var roles = userRegister.Role.Trim().Split(",");
                foreach (var role in roles)
                {
                    user.UserRole.Add(new UserRole()
                    {
                        RoleId = (int)Enum.Parse(typeof(RolesEnum), role, true)
                    });
                }

                await this.repository.AddAsync(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return CreateToken(user);
        }

        public async Task<AccessTokenResponse> RegisterExternalUsingFirebaseAsync(FirebaseRegisterExternal external)
        {
            FirebaseRegisterExternalValidation validation = new FirebaseRegisterExternalValidation();
            validation.ValidateAndThrow(external);

            FirebaseToken decodedToken = validation.ParsedToken;
            
            var claims = decodedToken.Claims;
            string email = claims["email"] + "";
            string name = claims["name"] + "";
            string avatar = claims["picture"] + "";

            var user = repository.GetUserByUsername(email);
            if(user == null)
            {
                user = new User()
                {
                    Email = email,
                    Username = email,
                    Fullname = name
                };
                user.UserRole.Add(new UserRole()
                {
                    RoleId = (int)RolesEnum.MEMBER
                });
                await this.repository.AddAsync(user);
            }

            return CreateToken(user);
        }

        private AccessTokenResponse CreateToken(User user)
        {
            return new AccessTokenResponse()
            {
                User = user.ToViewModel<UserViewModel>(),
                AccessToken = jwtTokenProvider.CreateAccesstoken(user),
                Roles = user.UserRole.Select(ur => ur.Role.Name).ToArray()
            };
        }
    }
}
