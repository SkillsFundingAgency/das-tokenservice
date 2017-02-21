﻿using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SFA.DAS.TokenService.Infrastructure.Http
{
    public interface IHttpClientWrapper
    {
        List<MediaTypeWithQualityHeaderValue> AcceptHeaders { get; set; }

        Task<T> Post<T>(string url, object content);
    }
}