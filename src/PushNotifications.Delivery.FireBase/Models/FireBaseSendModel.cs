﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PushNotifications.Contracts;

namespace PushNotifications.Delivery.FireBase.Models
{
    public class FireBaseSendModel
    {
        const long MAX_TTL = 1209600; // 4 weeks

        const long MAX_TTL_DAYS = 28; // 4 weeks

        public FireBaseSendModel(string token, FireBaseSendNotificationModel notification, Timestamp expiresAt, bool contentAvailable)
            : this(new List<string> { token }, notification, expiresAt, contentAvailable) { }

        public FireBaseSendModel(List<string> tokens, FireBaseSendNotificationModel notification, Timestamp expiresAt, bool contentAvailable)
        {
            if (ReferenceEquals(null, tokens) == true || tokens.Count == 0) throw new ArgumentException(nameof(tokens));
            if (ReferenceEquals(null, notification) == true) throw new ArgumentNullException(nameof(notification));
            if (ReferenceEquals(null, expiresAt) == true) throw new ArgumentNullException(nameof(expiresAt));

            RegistrationIds = new List<string>(tokens);
            Notification = notification;
            TTL = ExpirationTimeToSeconds(expiresAt);
            ContentAvailable = contentAvailable;
        }

        [JsonProperty(PropertyName = "Registration_ids")]
        public List<string> RegistrationIds { get; private set; }

        [JsonProperty(PropertyName = "Content_available")]
        public bool ContentAvailable { get; private set; }

        public FireBaseSendNotificationModel Notification { get; private set; }

        /// <summary>
        /// This parameter specifies how long (in seconds) the message should be kept in FCM storage if the device is offline.
        /// The maximum time to live supported is 4 weeks, and the default value is 4 weeks.
        /// </summary>
        [JsonProperty(PropertyName = "Time_to_live")]
        public long TTL { get; private set; }

        long ExpirationTimeToSeconds(Timestamp t)
        {
            var utcNow = DateTime.UtcNow;
            var difference = t.DateTime - utcNow;

            if (difference.TotalDays > MAX_TTL_DAYS)
                return MAX_TTL;

            return (long)difference.TotalSeconds;
        }
    }
}