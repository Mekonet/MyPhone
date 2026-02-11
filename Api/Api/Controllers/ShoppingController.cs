using Api.DataAccess;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ShoppingController : ControllerBase
  {
    readonly IDataAccess dataAccess;
    private readonly string DateFormat;
    public ShoppingController(IDataAccess dataAccess, IConfiguration configuration)
    {
      this.dataAccess = dataAccess;
      DateFormat = configuration["Constants:DateFormat"];
    }

    [HttpGet("GetCategoryList")]
    public IActionResult GetCategoryList()
    {
      var result = dataAccess.GetProductCategories();
      return Ok(result);
    }
    [HttpGet("GetProducts")]
    public IActionResult GetProducts(string category, string subcategory, int count)
    {
      var result = dataAccess.GetProducts(category, subcategory, count);
      return Ok(result);
    }
    [HttpGet("GetProduct/{id}")]
    public IActionResult GetProduct(int id)
    {
      var result = dataAccess.GetProduct(id);
      return Ok(result);
    }
    [HttpPost("RegisterUser")]
    public IActionResult RegisterUser([FromBody] User user)
    {
      user.CreatedAt = DateTime.Now.ToString(DateFormat);
      user.ModifiedAt = DateTime.Now.ToString(DateFormat);

      var result = dataAccess.InsertUser(user);

      string? message;
      if (result) message = "נרשמתה בהצלחה אנא התחבר";
      else message = "email not available";
      return Ok(message);
    }

    [HttpPost("LoginUser")]
    public IActionResult LoginUser([FromBody] User user)
    {
      var token = dataAccess.IsUserPresent(user.Email, user.Password);
      if (token == "") token = "invalid";
      return Ok(token);
    }

    [HttpPost("InsertReview")]
    public IActionResult InsertReview([FromBody] Review review)
    {
      review.CreatedAt = DateTime.Now.ToString(DateFormat);
      dataAccess.InsertReview(review);
      return Ok("inserted");
    }

    [HttpGet("GetProductReviews/{productId}")]
    public IActionResult GetProductReviews(int productId)
    {
      var result = dataAccess.GetProductReviews(productId);
      return Ok(result);
    }

    [HttpPost("InsertCartItem/{userid}/{productid}")]
    public IActionResult InsertCartItem(int userid, int productid)
    {
      var result = dataAccess.InsertCartItem(userid, productid);
      return Ok(result ? "inserted" : "not inserted");
    }

    [HttpGet("GetActiveCartOfUser/{id}")]
    public IActionResult GetActiveCartOfUser(int id)
    {
      var result = dataAccess.GetActiveCartOfUser(id);
      return Ok(result);
    }

    [HttpGet("GetAllPreviousCartOfUser/{id}")]
    public IActionResult GetAllPreviousCartOfUser(int id)
    {
      var result = dataAccess.GetAllPreviousCartsOfUser(id);
      return Ok(result);
    }

    [HttpGet("GetPaymentMethods")]
    public IActionResult GetPaymentMethods()
    {
      var result = dataAccess.GetPaymentMethods();
      return Ok(result);
    }

    [HttpPost("InsertPayment")]
    public IActionResult InsertPayment(Payment payment)
    {
      payment.CreatedAt = DateTime.Now.ToString();
      int id = dataAccess.InsertPayment(payment);
      return Ok(id.ToString());
    }

    [HttpPost("InsertOrder")]
    public IActionResult InsertOrder(Order order)
    {
      order.CreatedAt = DateTime.Now.ToString();
      var id = dataAccess.InsertOrder(order);
      return Ok(id.ToString());
    }
    [HttpDelete("RemoveProductFromCart/{productId}/{cartItemId}")]
    public IActionResult RemoveProductFromCart(int productId, int cartItemId)
    {
      bool removed = dataAccess.RemoveProductFromCart(productId, cartItemId);

      if (removed)
      {
        return Ok("Product removed from cart successfully");
      }
      else
      {
        return NotFound("Product not found in the cart");
      }
    }
    [HttpPut("UpdateProduct/{id}")]
    public IActionResult UpdateProduct(int id, [FromBody] Product product)
    {
      var existingProduct = dataAccess.GetProduct(id);
      if (existingProduct == null)
      {
        return NotFound("Product not found");
      }

      // מעדכן את הפרטים החדשים במוצר
      existingProduct.Title = product.Title;
      existingProduct.Description = product.Description;
      existingProduct.Price = product.Price;
      existingProduct.Quantity = product.Quantity;
      existingProduct.ImageName = product.ImageName;
      existingProduct.ProductCategory = product.ProductCategory;
      existingProduct.Offer = product.Offer;

      // מעדכן באחסון
      bool success = dataAccess.UpdateProduct(existingProduct);

      if (success)
      {
        return Ok(new { message = "Product updated successfully" });
      }
      else
      {
        return StatusCode(500, "Failed to update product");
      }
    }
    [HttpGet("SearchProducts")]
    public IActionResult SearchProducts(string query)
    {
      try
      {
        var products = dataAccess.SearchProducts(query);
        return Ok(products);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred while searching for products: {ex.Message}");
      }
    }

    [HttpPost("AddProduct")]
    public IActionResult AddProduct([FromBody] Product product)
    {
      bool success = dataAccess.AddProduct(product);

      if (success)
      {
        return Ok("Product added successfully");
      }
      else
      {
        return StatusCode(500, "Failed to add product");
      }
    }
    [HttpPut("UpdateProductQuantity/{productId}")]
    public IActionResult UpdateProductQuantity(int productId)
    {
      // מקבל את המוצר מהאחסון
      var product = dataAccess.GetProduct(productId);

      // בודק אם המצר קיים
      if (product == null)
      {
        return NotFound("Product not found");
      }
      if(product.Quantity > 0)
      // מעדכן כמות
      product.Quantity -= 1; 

      // מעדכן באחסון
      bool success = dataAccess.UpdateProduct(product);

      if (success)
      {
        return Ok(new { message = "Product quantity updated successfully" });
      }
      else
      {
        return StatusCode(500, "Failed to update product quantity");
      }
    }
    [HttpDelete("DeleteProduct/{productId}")]
    public IActionResult DeleteProduct(int productId)
    {
      // Check if the product exists
      var product = dataAccess.GetProduct(productId);
      if (product == null)
      {
        return NotFound("Product not found");
      }

      bool success = dataAccess.DeleteProduct(productId);
      if (success)
      {
        return Ok(new { message = "Product deleted successfully" });
      }
      else
      {
        return StatusCode(500, "Failed to delete product");
      }
    }
    [HttpPost("AddProductImage")]
    public IActionResult AddProductImage(int productId, string imageUrl)
    {
      // Call the data access layer to add the product image
      bool success = dataAccess.AddProductImage(productId, imageUrl);

      if (success)
      {
        return Ok(new { message = "Product image added successfully" });
      }
      else
      {
        return StatusCode(500, "Failed to add product image");
      }
    }
    [HttpGet("GetAllOrders")]
    public IActionResult GetAllOrders()
    {
      var orders = dataAccess.GetAllOrders();
      return Ok(orders);
    }
  }
}
