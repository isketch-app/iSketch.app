using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using iSketch.app.Classes;
using iSketch.app.Services;
using Microsoft.AspNetCore.Http;
using iSketch.app.Classes.OpenID;
using System.Text;

namespace iSketch.app.Endpoints {

    public static class OpenID
    {
        public static async Task Endpoint(HttpContext con)
        {
            Session session = con.InitializeSession();
            idP idP = Classes.OpenID.Helpers.GetIDP(session.db, Guid.Parse(con.Request.RouteValues["IdpID"].ToString()));
            if (idP == null)
            {
                con.Response.Redirect("/_Error/OpenID/idp-does-not-exist");
                return;
            }
            if (!con.Request.Query.ContainsKey("code") || con.Request.Query["code"] == "")
            {
                con.Response.Redirect("/_Error/OpenID/code-missing");
                return;
            }
            string code = con.Request.Query["code"];
            HttpClient hc = new();
            HttpRequestMessage msg = new();
            msg.RequestUri = new(idP.EndpointToken);
            msg.Method = HttpMethod.Post;
            if (idP.ClientSecret != null)
            {
                byte[] secret = Encoding.Default.GetBytes(HttpUtility.UrlEncode(idP.ClientID) + ":" + idP.ClientSecret);
                msg.Headers.Authorization = new("Basic", Convert.ToBase64String(secret));
            }
            FormUrlEncodedContent form = new(new Dictionary<string, string>() {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", idP.GetRedirectURI(session) },
                { "client_id", idP.ClientID }
            });
            msg.Content = new StreamContent(await form.ReadAsStreamAsync());
            msg.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            HttpResponseMessage hResponse = await hc.SendAsync(msg);
            try
            {
                hResponse.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                con.Response.Redirect("/_Error/OpenID/token-endpoint-bad-status-response?msg=" + e.Message);
                return;
            }
            Stream sResponse = await hResponse.Content.ReadAsStreamAsync();
            Dictionary<string, object> jResposne;
            try
            {
                jResposne = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(sResponse);
            }
            catch
            {
                con.Response.Redirect("/_Error/OpenID/jwt-deserialize-error");
                return;
            }
            if (!jResposne.TryGetValue("id_token", out object idToken))
            {
                sResponse.Position = 0;
                con.Response.Redirect("/_Error/OpenID/jwt-missing?idp_response=" + HttpUtility.UrlEncode(await new StreamReader(sResponse).ReadToEndAsync()));
                return;
            }
            JWT JWT;
            try
            {
                JWT = new(idToken.ToString());
            }
            catch
            {
                con.Response.Redirect("/_Error/OpenID/jwt-invalid");
                return;
            }
            TokenHandleResult result = Classes.OpenID.Helpers.HandleIdpIdToken(session, idP, JWT);
            if (result != TokenHandleResult.Success)
            {

                con.Response.Redirect("/_Error/OpenID/" + result.ToString());
                return;
            }
            con.Response.Redirect("/");
            await Task.CompletedTask;
        }
    }

}