using Api.Models;
using System.Data.SqlClient;

namespace Api.DataAccess
{
  public interface IDataAccess
  {
    List<ProductCategory> GetProductCategories();
    ProductCategory GetProductCategory(int id);
    Offer GetOffer(int id);
    List<Product> GetProducts(string category, string subcategory, int count);
    Product GetProduct(int id);
    bool InsertUser(User user);
    string IsUserPresent(string email, string password);
    void InsertReview(Review review);
    List<Review> GetProductReviews(int productId);
    User GetUser(int id);
    bool InsertCartItem(int userid, int productId);
    Cart GetActiveCartOfUser(int userid);
    Cart GetCart(int cartid);
    List<Cart> GetAllPreviousCartsOfUser(int userid);
    List<PaymentMethod> GetPaymentMethods();
    List<Order> GetAllOrders();
    int InsertPayment(Payment payment);
    int InsertOrder(Order order);
    void InsertMessage(string message);

    bool RemoveProductFromCart(int productId, int cartItemId);
    bool UpdateProduct(Product product);
    List<Product> SearchProducts(string query);
    bool AddProduct(Product product);
    bool UpdateProductQuantity(int productId, int quantity);
    bool DeleteProduct(int productId);
    bool AddProductImage(int productId, string imageUrl);
  }
}
