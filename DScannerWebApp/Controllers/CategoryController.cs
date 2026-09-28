using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RCommerce.Core;
using RCommerce.Infrastructure;
using MapsterMapper;

namespace RCommerce.WebApp.Controllers;

[Route("[Controller]")]
[Authorize(Policy = "AdminOnly")]
public class CategoryController : Controller
{
    private readonly RCommerceContext _context;
    private readonly IMapper _mapper;

    public CategoryController(RCommerceContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var categories = _mapper.Map<List<Models.Category>>(_context.Categories.ToList());
        return View(categories);
    }

    [HttpGet]
    [Route("New")]
    public IActionResult New()
    {
        var category = new Models.Category();
        return View(category);
    }

    [HttpPost]
    [Route("New")]
    public IActionResult Create(Models.Category dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CategoryName))
        {
            ModelState.AddModelError(string.Empty, "There were some errors in your form");
            return View("New", dto);
        }

        var category = _mapper.Map<Category>(dto);
        _context.Add(category);
        _context.SaveChanges();

        var categories = _mapper.Map<List<Models.Category>>(_context.Categories.ToList());
        return View("Index", categories);
    }

    [HttpGet]
    [Route("Edit/{id}")]
    public IActionResult Edit(int id)
    {
        var categoryEntity = _context.Categories.FirstOrDefault(p => p.Id == id);

        if (categoryEntity == null)
        {
            var categories = _mapper.Map<List<Models.Category>>(_context.Categories.ToList());
            return View("Index", categories);
        }
        else
        {
            var category = _mapper.Map<Models.Category>(categoryEntity);
            return View(category);
        }
    }

    [HttpPost]
    [Route("Edit/{id}")]
    public IActionResult Edit(int id, Category dto)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "There were some errors in your form");
            return View($"Edit/{id}", dto);
        }

        var categoryEntity = _context.Categories.FirstOrDefault(p => p.Id == id);

        if (categoryEntity == null)
        {
            return View("Index", _mapper.Map<List<Models.Category>>(_context.Categories.ToList()));
        }

        categoryEntity.CategoryName = dto.CategoryName;

        _context.Categories.Update(categoryEntity);
        _context.SaveChanges();


        var products = _mapper.Map<List<Models.Product>>(_context.Categories.ToList());
        return View("Index", products);
    }

    [HttpDelete]
    [Route("Delete/{id}")]
    public JsonResult Delete(int id)
    {
        var categoryEntity = _context.Categories.FirstOrDefault(p => p.Id == id);
        if (categoryEntity == null)
            return Json(new { success = true, message = "Already Deleted" });

        _context.Categories.Remove(categoryEntity);
        _context.SaveChanges();

        return Json(new { success = true, message = "Delete success" });
    }
}
