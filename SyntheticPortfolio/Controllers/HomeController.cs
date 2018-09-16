using SyntheticPortfolio.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SyntheticPortfolio.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            string ApiUrl = ConfigurationManager.AppSettings["APIURL"].ToString();
            DataServiceAPI.Initialize(ApiUrl);
        }
        [HttpGet]
        public ActionResult Login(string returnURL)
        {
            var userinfo = new LoginVM();

            try
            {
                // We do not want to use any existing identity information
                EnsureLoggedOut();

                // Store the originating URL so we can attach it to a form field
                userinfo.ReturnURL = returnURL;

                return View(userinfo);
            }
            catch
            {
                throw;
            }
        }
        //GET: EnsureLoggedOut
        private void EnsureLoggedOut()
        {
            // If the request is (still) marked as authenticated we send the user to the logout action
            if (Request.IsAuthenticated)
                Logout();
        }

        //POST: Logout
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // First we clean the authentication ticket like always
            //required NameSpace: using System.Web.Security;
            FormsAuthentication.SignOut();

            // Second we clear the principal to ensure the user does not retain any authentication
            //required NameSpace: using System.Security.Principal;
            HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

            Session.Clear();
            System.Web.HttpContext.Current.Session.RemoveAll();

            // Last we redirect to a controller/action that requires authentication to ensure a redirect takes place
            // this clears the Request.IsAuthenticated flag since this triggers a new request
            return RedirectToLocal();

        }
        //GET: SignInAsync
        private void SignInRemember(string userName, bool isPersistent = false)
        {
            // Clear any lingering authencation data
            FormsAuthentication.SignOut();

            // Write the authentication cookie
            FormsAuthentication.SetAuthCookie(userName, isPersistent);
        }

        //GET: RedirectToLocal
        private ActionResult RedirectToLocal(string returnURL = "")
        {
            try
            {
                // If the return url starts with a slash "/" we assume it belongs to our site
                // so we will redirect to this "action"
                if (!string.IsNullOrWhiteSpace(returnURL) && Url.IsLocalUrl(returnURL))
                    return Redirect(returnURL);

                // If we cannot verify if the url is local to our host we redirect to a default location
                return RedirectToAction("Index", "Main");
            }
            catch
            {
                throw;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM entity)
        {
            try
            {
                if (entity.Username==null ||entity.Password==null)
                {
                    TempData["ErrorMSG"] = "Access Denied! Invalid Credential";
                    return View(entity);
                }
                if (!ModelState.IsValid)
                    return View(entity);
                string result = DataServiceAPI.DownloadData($"/IB/Authen/{entity.Username}/{entity.Password}");
                bool isLogin = Convert.ToBoolean(result);
                if (isLogin)
                {//Login Success
                 //For Set Authentication in Cookie (Remeber ME Option)
                    SignInRemember(entity.Username, entity.isRemember);

                    //Set A Unique ID in session
                    Session["UserID"] = entity.Password;

                    // If we got this far, something failed, redisplay form
                    // return RedirectToAction("Index", "Dashboard");
                    return RedirectToLocal(entity.ReturnURL);
                }
                else
                {
                    TempData["ErrorMSG"] = "Access Denied! Wrong Credential";
                    return View(entity);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMSG"] = ex.Message;
                return View(entity);
            }

        }



        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}