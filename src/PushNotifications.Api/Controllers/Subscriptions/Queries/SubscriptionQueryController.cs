﻿using Elders.Cronus.DomainModeling;
using Elders.Web.Api;
using System.Web.Http;
using PushNotifications.Contracts;
using Discovery.Contracts;
using Elders.Cronus.DomainModeling.Projections;
using PushNotifications.Projections;
using PushNotifications.Api.Controllers.Subscriptions.Models;
using PushNotifications.Projections.General;

namespace PushNotifications.Api.Controllers.Subscriptions.Queries
{
    [RoutePrefix("Subscriptions")]
    public class SubscriptionQueryController : ApiController
    {
        public IProjectionRepository Projections { get; set; }

        /// <summary>
        /// Provides registered tokens for user for all delivery providers e.g FireBase, Pushy etc
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet, Route("SubscriberTokens"), Discoverable("SubscriberTokens", "v1")]
        public IHttpActionResult GetSubscriberTokens(SubscriberTokensModel model)
        {
            if (Urn.IsUrn(model.SubscriberId) == false)
                return Ok(new ResponseResult());

            var urn = StringTenantUrn.Parse(model.SubscriberId);
            var subscriberId = new SubscriberId(urn.Id, urn.Tenant);

            var projectionReponse = Projections.Get<SubscriberTokensForAllProvidersProjection>(subscriberId);
            if (projectionReponse.Success == true)
            {
                return Ok(new ResponseResult<SubscriberTokens>(projectionReponse.Projection.State));
            }

            return Ok(new ResponseResult());
        }
    }
}
