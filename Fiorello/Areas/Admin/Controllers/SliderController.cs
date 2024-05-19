using Fiorello.DAL;
using Fiorello.Helpers.Extentions;
using Fiorello.Models;
using Fiorello.ViewModels.Sliders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fiorello.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SliderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public SliderController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Slider> sliders = await _context.Sliders.ToListAsync();
            return View(sliders);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderCreateVM create)
        {
            if (!ModelState.IsValid) return View();
            foreach (var item in create.Images)
            {

                if (!item.CheckFileType("image/"))
                {
                    ModelState.AddModelError("Images", "File must be Image Format");
                    return View();
                }
                if (!item.CheckFileSize(200))
                {

                    ModelState.AddModelError("Images", "Max File Capacity mut be 300KB");
                    return View();
                }
            }
            foreach (var item in create.Images)
            {
                string fileName = Guid.NewGuid().ToString() + "-" + item.FileName;
                string path = Path.Combine(_env.WebRootPath, "img", fileName);
                await item.SaveFileToLocalAsync(path);
                await _context.Sliders.AddAsync(new Slider { Name = fileName });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id is null) return BadRequest();
            Slider slider = await _context.Sliders.Where(c => c.Id == id).FirstOrDefaultAsync();
            if (slider == null) return NotFound();
            SliderDetailVM model = new()
            {
                Id = slider.Id,
                Name = slider.Name

            };
            return View(model);
        }
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Slider slide = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);

            if (slide is null) return NotFound();

            slide.Name.DeleteFile(_env.WebRootPath, "img");

            _context.Sliders.Remove(slide);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
    }
}
