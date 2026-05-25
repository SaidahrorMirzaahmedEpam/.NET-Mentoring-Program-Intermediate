using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProfileSample.DAL;
using ProfileSample.Models;

namespace ProfileSample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Use a scoped context and a single query to fetch required fields
            using (var context = new ProfileSampleEntities())
            {
                var model = context.ImgSources
                    .AsNoTracking()
                    .OrderBy(x => x.Id)
                    .Take(20)
                    .Select(x => new ImageModel
                    {
                        Name = x.Name,
                        Data = x.Data
                    })
                    .ToList();

                return View(model);
            }
        }

        public ActionResult Convert()
        {
            var files = Directory.GetFiles(Server.MapPath("~/Content/Img"), "*.jpg");

            using (var context = new ProfileSampleEntities())
            {
                foreach (var file in files)
                {
                    // Read the file into memory in a single call
                    byte[] buff = System.IO.File.ReadAllBytes(file);

                    var entity = new ImgSource()
                    {
                        Name = Path.GetFileName(file),
                        Data = buff,
                    };

                    context.ImgSources.Add(entity);
                }

                // Persist all added entities in a single SaveChanges call
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}