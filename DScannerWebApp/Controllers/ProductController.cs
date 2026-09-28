using RCommerce.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MapsterMapper;
using RCommerce.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RCommerce.WebApp.Controllers;

[Route("[Controller]")]
[Authorize(Policy = "AdminOnly")]
public class ProductController : Controller
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IMapper _mapper;
    private readonly RCommerceContext _context;

    public ProductController(IWebHostEnvironment hostEnvironment, RCommerceContext context, IMapper mapper)
    {
        _hostEnvironment = hostEnvironment;
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var products = _mapper.Map<List<Models.Product>>(_context.Products.Include(p => p.Category).ToList());
        return View(products);
    }

    [HttpGet]
    [Route("New")]
    public IActionResult New()
    {
        var product = new Models.Product();
        LoadCategories(product);

        return View(product);
    }

    [HttpPost]
    [Route("New")]
    public IActionResult Create(Models.Product dto)
    {
        var product = _mapper.Map<Product>(dto);
        var category = _context.Categories.Find(dto.CategoryId);
        product.Category = category;

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "There were some errors in your form");
            LoadCategories(dto);
            return View("New", dto);
        }

        //SaveImage(dto);

        _context.Add(product);
        _context.SaveChanges();

        var products = _mapper.Map<List<Models.Product>>(_context.Products.ToList());
        return View("Index", products);
    }

    [HttpGet]
    [Route("Edit/{id}")]
    public IActionResult Edit(int id)
    {
        var prod = _context.Products.FirstOrDefault(p => p.Id == id);

        if (prod == null)
        {
            var products = _mapper.Map<List<Models.Product>>(_context.Products.Include(p => p.Category));
            return View("Index", products);
        }
        else
        {
            var product = _mapper.Map<Models.Product>(prod);
            LoadCategories(product);
            return View(product);
        }
    }

    [HttpPost]
    [Route("Edit/{id}")]
    public IActionResult Edit(int id, Models.Product dto)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "There were some errors in your form");
            LoadCategories(dto);
            return View($"Edit/{id}", dto);
        }

        var prod = _context.Products.FirstOrDefault(p => p.Id == id);

        if (prod == null)
        {
            return View("Index", _mapper.Map<List<Models.Product>>(_context.Products.Include(p => p.Category)));
        }

        //var oldFileRelativePath = prod.ImagePath;

        //if (dto.ProducImage == null)
        //{
        //    dto.ImagePath = oldFileRelativePath;
        //}
        //else
        //{
        //    if (!string.IsNullOrWhiteSpace(oldFileRelativePath))
        //    {
        //        var olfFileFullPath = Path.Combine(hostEnvironment.WebRootPath, oldFileRelativePath);
        //        if (System.IO.File.Exists(olfFileFullPath))
        //            System.IO.File.Delete(olfFileFullPath);
        //    }

        //    SaveImage(dto);
        //}

        prod.ProductName = dto.ProductName;
        prod.ProductDescription = dto.ProductDescription;
        prod.Price = dto.Price;
        prod.IsAvailable = dto.IsAvailable;
        prod.ImagePath = dto.ImagePath;

        var category = _context.Categories.Find(dto.CategoryId);
        prod.Category = category;

        _context.Products.Update(prod);
        _context.SaveChanges();


        var products = _mapper.Map<List<Models.Product>>(_context.Products.Include(p => p.Category));
        return View("Index", products);
    }

    [HttpDelete]
    [Route("Delete/{id}")]
    public JsonResult Delete(int id)
    {
        var prod = _context.Products.FirstOrDefault(p => p.Id == id);
        if (prod == null)
            return Json(new { success = true, message = "Already Deleted" });

        //if (!string.IsNullOrWhiteSpace(prod.ImagePath))
        //{
        //    var filePath = Path.Combine(hostEnvironment.WebRootPath, prod.ImagePath);

        //    if (System.IO.File.Exists(filePath))
        //        System.IO.File.Delete(filePath);
        //}

        _context.Products.Remove(prod);
        _context.SaveChanges();

        return Json(new { success = true, message = "Delete success" });
    }

    //private void SaveImage(ProductVM dto)
    //{
    //    if (dto.ProducImage == null)
    //        return;

    //    var imgFolderPath = Path.Combine(hostEnvironment.WebRootPath, imgFolderName);

    //    if (!Directory.Exists(imgFolderPath))
    //        Directory.CreateDirectory(imgFolderPath);

    //    var fileName = Guid.NewGuid() + Path.GetExtension(dto.ProducImage.FileName);
    //    var imgFullPath = Path.Combine(imgFolderPath, fileName);

    //    using (var fileStream = new FileStream(imgFullPath, FileMode.Create))
    //        dto.ProducImage.CopyTo(fileStream);

    //    dto.ImagePath = Path.Combine(imgFolderName, fileName);
    //}

    public void LoadCategories(Models.Product product)
    {

        var availableCategories = _mapper.Map<List<Models.Category>>(_context.Categories.ToList());

        availableCategories.ForEach(x => product.CategoryList.Add(
            new SelectListItem
            {
                Text = x.CategoryName,
                Value = x.Id.ToString()
            }));
    }
}
