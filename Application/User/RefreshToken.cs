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
    public class RefreshToken
    {
         public class Query : IRequest<User>
         {
            public string Username{get; set;}
            public string Token {get;set;}
            public string RefreshToken { get; set; }
        }

        public class Handler : IRequestHandler<Query, User>
        {
            private readonly UserManager<AppUser> _userManager;
            private readonly IJwtGenerator _jwtGenerator;
            public Handler(UserManager<AppUser> userManager,  IJwtGenerator jwtGenerator)
            {
                _jwtGenerator = jwtGenerator;
                _userManager = userManager;
            }

            public async Task<User> Handle(Query request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByNameAsync(request.Username);

                if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry< DateTime.Now)
                    throw new RestException(HttpStatusCode.BadRequest, new {User="Problem validating refresh token"});
                
                user.RefreshToken = _jwtGenerator.GenerateRefreshToken();
                user.RefreshTokenExpiry = DateTime.Now.AddDays(30);
                await _userManager.UpdateAsync(user);


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