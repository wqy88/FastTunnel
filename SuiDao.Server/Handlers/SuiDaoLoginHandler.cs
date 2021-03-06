﻿using FastTunnel.Core.Exceptions;
using FastTunnel.Core.Handlers;
using FastTunnel.Core.Models;
using Newtonsoft.Json.Linq;
using SuiDao.Client;
using System;
using System.Collections.Generic;

namespace SuiDao.Server
{
    public class SuiDaoLoginHandler : ILoginHandler
    {
        public LogInRequest GetConfig(object content)
        {
            var key = (content as JObject)["key"].ToString();
            var res = HttpHelper.PostAsJson("https://api1.suidao.io/api/Client/GetTunnelByKey", $"{{ \"key\":\"{key}\"}}").Result;
            var jobj = JObject.Parse(res);
            if ((bool)jobj["success"] == true)
            {
                var tunnels = jobj["data"].ToObject<IEnumerable<Tunnel>>();
                var Webs = new List<WebConfig>();
                var SSH = new List<SSHConfig>();

                foreach (var tunnel in tunnels)
                {
                    if (tunnel.app_type == 1) // web
                    {
                        Webs.Add(new WebConfig
                        {
                            LocalIp = tunnel.local_ip,
                            LocalPort = tunnel.local_port,
                            SubDomain = tunnel.sub_domain
                        });
                    }
                    else if (tunnel.app_type == 2)
                    {
                        SSH.Add(new SSHConfig
                        {
                            LocalIp = tunnel.local_ip,
                            LocalPort = tunnel.local_port,
                            RemotePort = tunnel.remote_port,
                            Protocol = Protocol.TCP
                        });
                    }
                }

                return new LogInRequest
                {
                    SSH = SSH,
                    Webs = Webs,
                };
            }
            else
            {
                throw new APIErrorException(jobj["errorMsg"].ToString());
            }
        }
    }
}
