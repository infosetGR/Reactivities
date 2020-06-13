using System.Threading;
using System.Threading.Tasks;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Application.Interfaces;
using Application.Errors;
using System.Net;
using System.Linq;
using System;

namespace Application.User
{
    public class ExternalLogin
    {
          public class Query : IRequest<User>
        {
            public string AccessToken { get; set; }
        }

        // public class QueryValidator : AbstractValidator<Query>
        // {
        //     public QueryValidator()
        //     {
        //         RuleFor(x => x.Email).NotEmpty();
        //         RuleFor(x => x.Password).NotEmpty();
        //     }
        // }

        public class Handler : IRequestHandler<Query, User>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly IFacebookAccessor _facebookAccessor;
            private readonly IJwtGenerator _jwtGenerator;
            public Handler(UserManager<AppUser> userManager, IFacebookAccessor facebookAccessor, IJwtGenerator jwtGenerator)
            {
                _jwtGenerator = jwtGenerator;
                _facebookAccessor = facebookAccessor;
                _userManager = userManager;
            }

            public async Task<User> Handle(Query request, CancellationToken cancellationToken)
            {
                var userInfo = await _facebookAccessor.FacebookLogin(request.AccessToken);

                if (userInfo == null)
                    throw new RestException(HttpStatusCode.BadRequest, new {User="Problem validating token"});

                var user = await _userManager.FindByIdAsync(userInfo.Id);

                if (user == null) {
                    user = new AppUser {
                        DisplayName= userInfo.Name,
                        Id=userInfo.Id,
                        Email=userInfo.Id.ToString()+"@Facebook.com",//userInfo.Email,
                        UserName="fb_"+userInfo.Id,
                        RefreshToken = _jwtGenerator.GenerateRefreshToken(),
                        RefreshTokenExpiry = DateTime.Now.AddDays(30)
                    };

                    var photo = new Photo {
                        Id="fb_"+userInfo.Id,
                        Url=userInfo.Picture.Data.Url,
                        IsMain=true
                    };
                    user.Photos.Add(photo);
                    
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)   {
                        throw new RestException(HttpStatusCode.BadRequest, new {User="Problem creating token"});
                    }
                    
                }
                return new User  {
                        DisplayName = user.DisplayName,
                        Token = _jwtGenerator.CreateToken(user),
                        Username = user.UserName,
                        Image = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                        RefreshToken = user.RefreshToken

                    };
                
            }
        }
    }
}