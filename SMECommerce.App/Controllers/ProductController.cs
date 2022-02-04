using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMECommerce.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMECommerce.App.Controllers
{
    public class ProductController : Controller
    {
        IProductService _productService;
        ICategoryService _categoryService; 
        public ProductController(ICategoryService categoryService, IProductService productService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [Authorize("AgeMinimum16")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
