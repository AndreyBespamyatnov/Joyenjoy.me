using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using PhotoBooth.WebApp.Models;

namespace PhotoBooth.WebApp.Controllers
{
    using System;
    using System.Web.Mvc;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {   
            return this.View();
        }

        public ActionResult Contact()
        {
            return this.View();
        }

        public ActionResult Fotobox()
        {
            return this.View();
        }

        public ActionResult Brandbox()
        {
            return this.View();
        }

        public ActionResult Design()
        {
            return this.View();
        }

        public ActionResult Instabox()
        {
            return this.View();
        }

        public ActionResult Socialmatic()
        {
            return this.View();
        }

        // GET: Home/BuyStuff
        public ActionResult BuyStuff()
        {
            //do somethingthing... oops, an error
            throw new NotImplementedException("Buy Stuff isn't working");
        }

        public async Task<ActionResult> EmailRequest(EmailRequest model)
        {
            var body = "<p>Заявка с сайта joyenjoy.me</p><p>Имя:{0}</p><p>Телефон:{1}</p><p>Email:{2}</p><p>Дата:{3}</p><p>Заказанные услуги:{4}</p>";
            var message = new MailMessage();
            message.To.Add(new MailAddress("info@joyenjoy.me"));  // replace with valid value 
            //message.To.Add(new MailAddress("adminsgz@gmail.com"));  // replace with valid value 
            message.From = new MailAddress("info@joyenjoy.me");  // replace with valid value
            message.Subject = "Заявка с сайта joyenjoy.me";
            message.Body = string.Format(body, model.name, model.phone, model.email, model.datetime, model.service);
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
            }

            return this.View("Index");
        }
    }
}