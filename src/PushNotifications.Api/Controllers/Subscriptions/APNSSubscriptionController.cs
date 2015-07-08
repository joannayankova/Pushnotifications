﻿using Elders.Cronus.DomainModeling;
using Elders.Web.Api;
using PushNotifications.Contracts.Subscriptions;
using PushNotifications.Contracts.Subscriptions.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using Thinktecture.IdentityModel.WebApi;

namespace PushNotifications.Api.Controllers.Subscriptions
{
    [ScopeAuthorize("owner")]
    [RoutePrefix("Subscriptions/APNSSubscription")]
    public class APNSSubscriptionController : ApiController
    {
        public IPublisher<ICommand> Publisher { get; set; }

        [HttpPost, Route("Subscribe")]
        public IHttpActionResult Subscribe(SubscriptionModel model)
        {
            var result = new ResponseResult(Constants.InvalidCommand);

            var command = model.AsSubscribeCommand();
            if (command.IsValid())
            {
                result = Publisher.Publish(command)
                    ? new ResponseResult<ResponseResult>(new ResponseResult())
                    : new ResponseResult(Constants.CommandPublishFailed);
            }
            return result.IsSuccess
                ? this.Accepted(result)
                : this.NotAcceptable(result);
        }

        [HttpPost, Route("UnSubscribe")]
        public IHttpActionResult UnSubscribe(SubscriptionModel model)
        {
            var result = new ResponseResult(Constants.InvalidCommand);

            var command = model.AsUnSubscribeCommand();
            if (command.IsValid())
            {
                result = Publisher.Publish(command)
                    ? new ResponseResult<ResponseResult>(new ResponseResult())
                    : new ResponseResult(Constants.CommandPublishFailed);
            }
            return result.IsSuccess
                ? this.Accepted(result)
                : this.NotAcceptable(result);
        }
    }

    public class SubscriptionModel
    {
        [AuthorizeClaim(AuthorizeClaimType.Subject)]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }

        public SubscribeForAPNS AsSubscribeCommand()
        {
            var id = new APNSSubscriptionId(Token);
            return new SubscribeForAPNS(id, UserId, Token);
        }

        public UnSubscribeFromAPNS AsUnSubscribeCommand()
        {
            var id = new APNSSubscriptionId(Token);
            return new UnSubscribeFromAPNS(id, UserId, Token);
        }
    }
}
