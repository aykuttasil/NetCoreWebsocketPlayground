using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace websocketplayground
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();
            app.Use(async (ctx, nextMsg) =>
            {    
                Console.WriteLine("Web Socket is listening");
                if (ctx.Request.Path == "/socket")
                {
                    if (ctx.WebSockets.IsWebSocketRequest)
                    {
                        var wSocket = await ctx.WebSockets.AcceptWebSocketAsync();
                        await Talk(ctx, wSocket);
                    }
                    else
                    {
                        ctx.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await nextMsg();
                }
            });

            app.UseFileServer();
        }



        private async Task Talk(HttpContext hContext, WebSocket wSocket)
        {
            var bag = new byte[1024];
            var result = await wSocket.ReceiveAsync(new ArraySegment<byte>(bag), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var incomingMessage = System.Text.Encoding.UTF8.GetString(bag, 0, result.Count);
                Console.WriteLine("\nClient says that '{0}'\n", incomingMessage);
                var rnd = new Random();
                var number = rnd.Next(1, 100);
                string message = string.Format("Your lucky Number is '{0}'. Don't remember that :)", number.ToString());
                byte[] outgoingMessage = System.Text.Encoding.UTF8.GetBytes(message);
                await wSocket.SendAsync(new ArraySegment<byte>(outgoingMessage, 0, outgoingMessage.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                result = await wSocket.ReceiveAsync(new ArraySegment<byte>(bag), CancellationToken.None);
            }
            await wSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
