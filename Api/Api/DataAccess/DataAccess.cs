using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.DataAccess
{
  public class DataAccess : IDataAccess
  {
    private readonly IConfiguration configuration;
    private readonly string dbconnection;
    private readonly string dateformat;

    public DataAccess(IConfiguration configuration)
    {
      this.configuration = configuration;
      dbconnection = this.configuration["ConnectionStrings:DB"];
      dateformat = this.configuration["Constants:DateFormat"];
    }

    public Cart GetActiveCartOfUser(int userid)
    {
      var cart = new Cart();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };
        connection.Open();

        string query = "SELECT COUNT(*) From Carts WHERE UserId=" + userid + " AND Ordered='false';";
        command.CommandText = query;

        int count = (int)command.ExecuteScalar();
        if (count == 0)
        {
          return cart;
        }

        query = "SELECT CartId From Carts WHERE UserId=" + userid + " AND Ordered='false';";
        command.CommandText = query;
        int cartid = (int)command.ExecuteScalar();

        query = "SELECT * From CartItems WHERE CartId=" + cartid + ";";
        command.CommandText = query;

        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          CartItem item = new()
          {
            Id = (int)reader["CartItemId"],
            Product = GetProduct((int)reader["ProductId"])
          };
          cart.CartItems.Add(item);
        }

        cart.Id = cartid;
        cart.User = GetUser(userid);
        cart.Ordered = false;
        cart.OrderedOn = "";
      }
      return cart;
    }

    public List<Cart> GetAllPreviousCartsOfUser(int userid)
    {
      var carts = new List<Cart>();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "SELECT CartId From Carts WHERE UserId=" + userid + " AND Ordered='true';";
        command.CommandText = query;
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          var cartid = (int)reader["CartId"];
          carts.Add(GetCart(cartid));
        }
      }
      return carts;
    }

    public Cart GetCart(int cartid)
    {
      var cart = new Cart();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };
        connection.Open();

        string query = "SELECT * FROM CartItems WHERE CartId=" + cartid + ";";
        command.CommandText = query;

        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          CartItem item = new()
          {
            Id = (int)reader["CartItemId"],
            Product = GetProduct((int)reader["ProductId"])
          };
          cart.CartItems.Add(item);
        }
        reader.Close();

        query = "SELECT * FROM Carts WHERE CartId=" + cartid + ";";
        command.CommandText = query;
        reader = command.ExecuteReader();
        while (reader.Read())
        {
          cart.Id = cartid;
          cart.User = GetUser((int)reader["UserId"]);
          cart.Ordered = bool.Parse((string)reader["Ordered"]);
          cart.OrderedOn = (string)reader["OrderedOn"];
        }
        reader.Close();
      }
      return cart;
    }

    public Offer GetOffer(int id)
    {
      var offer = new Offer();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };
        string query = "SELECT * FROM Offers WHERE OfferId=" + id + ";";
        command.CommandText = query;

        connection.Open();
        SqlDataReader r = command.ExecuteReader();
        while (r.Read())
        {
          offer.Id = (int)r["OfferId"];
          offer.Title = (string)r["Title"];
          offer.Discount = (int)r["Discount"];
        }
      }
      return offer;
    }

    public List<PaymentMethod> GetPaymentMethods()
    {
      var reault = new List<PaymentMethod>();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "SELECT * FROM PaymentMethods;";
        command.CommandText = query;

        connection.Open();

        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          PaymentMethod paymentMethod = new()
          {
            Id = (int)reader["PaymentMethodId"],
            Type = (string)reader["Type"],
            Provider = (string)reader["Provider"],
            Available = bool.Parse((string)reader["Available"]),
            Reason = (string)reader["Reason"]
          };
          reault.Add(paymentMethod);
        }
      }
      return reault;
    }

    public Product GetProduct(int id)
    {
      var product = new Product();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "SELECT * FROM Products WHERE ProductId=" + id + ";";
        command.CommandText = query;
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          product.Id = (int)reader["ProductId"];
          product.Title = (string)reader["Title"];
          product.Description = (string)reader["Description"];
          product.Price = (double)reader["Price"];
          product.Quantity = (int)reader["Quantity"];
          product.ImageName = (string)reader["ImageName"];

          if (product.Quantity < 5)
          {
            // Generate a message for low stock
            string lowStockMessage = $"Low stock for product '{product.Title}'. Quantity: {product.Quantity}";
            // Store the message in the Messages table
            InsertMessage(lowStockMessage);
          }

          var categoryid = (int)reader["CategoryId"];
          product.ProductCategory = GetProductCategory(categoryid);

          var offerid = (int)reader["OfferId"];
          product.Offer = GetOffer(offerid);
        }
      }
      return product;
    }
    public List<ProductCategory> GetProductCategories()
    {
      var productCategories = new List<ProductCategory>();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };
        string query = "SELECT * FROM ProductCategories;";
        command.CommandText = query;

        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          var category = new ProductCategory()
          {
            Id = (int)reader["CategoryId"],
            Category = (string)reader["Category"],
            SubCategory = (string)reader["SubCategory"]
          };
          productCategories.Add(category);
        }
      }
      return productCategories;
    }
    public ProductCategory GetProductCategory(int id)
    {
      var productCategory = new ProductCategory();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "SELECT * FROM ProductCategories WHERE CategoryId=" + id + ";";
        command.CommandText = query;

        connection.Open();
        SqlDataReader r = command.ExecuteReader();
        while (r.Read())
        {
          productCategory.Id = (int)r["CategoryId"];
          productCategory.Category = (string)r["Category"];
          productCategory.SubCategory = (string)r["SubCategory"];
        }
      }
      return productCategory;
    }

    public List<Review> GetProductReviews(int productId)
    {
      var reviews = new List<Review>();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "SELECT * FROM Reviews WHERE ProductId=" + productId + ";";
        command.CommandText = query;

        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          var review = new Review()
          {
            Id = (int)reader["ReviewId"],
            Value = (string)reader["Review"],
            CreatedAt = (string)reader["CreatedAt"]
          };

          var userid = (int)reader["UserId"];
          review.User = GetUser(userid);

          var productid = (int)reader["ProductId"];
          review.Product = GetProduct(productid);

          reviews.Add(review);
        }
      }
      return reviews;
    }

    public List<Product> GetProducts(string category, string subcategory, int count)
    {
      var products = new List<Product>();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "SELECT TOP " + count + " * FROM Products WHERE CategoryId=(SELECT CategoryId FROM ProductCategories WHERE Category=@c AND SubCategory=@s) ORDER BY newid();";
        command.CommandText = query;
        command.Parameters.Add("@c", System.Data.SqlDbType.NVarChar).Value = category;
        command.Parameters.Add("@s", System.Data.SqlDbType.NVarChar).Value = subcategory;

        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          var product = new Product()
          {
            Id = (int)reader["ProductId"],
            Title = (string)reader["Title"],
            Description = (string)reader["Description"],
            Price = (double)reader["Price"],
            Quantity = (int)reader["Quantity"],
            ImageName = (string)reader["ImageName"]
          };

          var categoryid = (int)reader["CategoryId"];
          product.ProductCategory = GetProductCategory(categoryid);

          var offerid = (int)reader["OfferId"];
          product.Offer = GetOffer(offerid);

          products.Add(product);
        }
      }
      return products;
    }

    public User GetUser(int id)
    {
      var user = new User();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "SELECT * FROM Users WHERE UserId=" + id + ";";
        command.CommandText = query;

        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          user.Id = (int)reader["UserId"];
          user.FirstName = (string)reader["FirstName"];
          user.LastName = (string)reader["LastName"];
          user.Email = (string)reader["Email"];
          user.Address = (string)reader["Address"];
          user.Mobile = (string)reader["Mobile"];
          user.Password = (string)reader["Password"];
          user.CreatedAt = (string)reader["CreatedAt"];
          user.ModifiedAt = (string)reader["ModifiedAt"];
          user.IsAdmin = (bool)reader["IsAdmin"];
        }
      }
      return user;
    }
    public bool InsertCartItem(int userId, int productId)
    {
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        connection.Open();
        string query = "SELECT COUNT(*) FROM Carts WHERE UserId=" + userId + " AND Ordered='false';";
        command.CommandText = query;
        int count = (int)command.ExecuteScalar();
        if (count == 0)
        {
          query = "INSERT INTO Carts (UserId, Ordered, OrderedOn) VALUES (" + userId + ", 'false', '');";
          command.CommandText = query;
          command.ExecuteNonQuery();
        }

        query = "SELECT CartId FROM Carts WHERE UserId=" + userId + " AND Ordered='false';";
        command.CommandText = query;
        int cartId = (int)command.ExecuteScalar();


        query = "INSERT INTO CartItems (CartId, ProductId) VALUES (" + cartId + ", " + productId + ");";
        command.CommandText = query;
        command.ExecuteNonQuery();
        return true;
      }
    }


    public int InsertOrder(Order order)
    {
      int value = 0;

      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = "INSERT INTO Orders (UserId, CartId, PaymentId, CreatedAt) values (@uid, @cid, @pid, @cat);";

        command.CommandText = query;
        command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = order.User.Id;
        command.Parameters.Add("@cid", System.Data.SqlDbType.Int).Value = order.Cart.Id;
        command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = order.CreatedAt;
        command.Parameters.Add("@pid", System.Data.SqlDbType.Int).Value = order.Payment.Id;

        connection.Open();
        value = command.ExecuteNonQuery();

        if (value > 0)
        {
          query = "UPDATE Carts SET Ordered='true', OrderedOn='" + DateTime.Now.ToString(dateformat) + "' WHERE CartId=" + order.Cart.Id + ";";
          command.CommandText = query;
          command.ExecuteNonQuery();

          query = "SELECT TOP 1 Id FROM Orders ORDER BY Id DESC;";
          command.CommandText = query;
          value = (int)command.ExecuteScalar();
        }
        else
        {
          value = 0;
        }
      }

      return value;
    }
    public int InsertPayment(Payment payment)
    {
      int value = 0;
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = @"INSERT INTO Payments (PaymentMethodId, UserId, TotalAmount, ShippingCharges, AmountReduced, AmountPaid, CreatedAt) 
                                VALUES (@pmid, @uid, @ta, @sc, @ar, @ap, @cat);";

        command.CommandText = query;
        command.Parameters.Add("@pmid", System.Data.SqlDbType.Int).Value = payment.PaymentMethod.Id;
        command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = payment.User.Id;
        command.Parameters.Add("@ta", System.Data.SqlDbType.NVarChar).Value = payment.TotalAmount;
        command.Parameters.Add("@sc", System.Data.SqlDbType.NVarChar).Value = payment.ShipingCharges;
        command.Parameters.Add("@ar", System.Data.SqlDbType.NVarChar).Value = payment.AmountReduced;
        command.Parameters.Add("@ap", System.Data.SqlDbType.NVarChar).Value = payment.AmountPaid;
        command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = payment.CreatedAt;

        connection.Open();
        value = command.ExecuteNonQuery();

        if (value > 0)
        {
          query = "SELECT TOP 1 Id FROM Payments ORDER BY Id DESC;";
          command.CommandText = query;
          value = (int)command.ExecuteScalar();
        }
        else
        {
          value = 0;
        }
      }
      return value;
    }

    public void InsertReview(Review review)
    {
      using SqlConnection connection = new(dbconnection);
      SqlCommand command = new()
      {
        Connection = connection
      };

      string query = "INSERT INTO Reviews (UserId, ProductId, Review, CreatedAt) VALUES (@uid, @pid, @rv, @cat);";
      command.CommandText = query;
      command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = review.User.Id;
      command.Parameters.Add("@pid", System.Data.SqlDbType.Int).Value = review.Product.Id;
      command.Parameters.Add("@rv", System.Data.SqlDbType.NChar).Value = review.Value;
      command.Parameters.Add("@cat", System.Data.SqlDbType.NChar).Value = review.CreatedAt;

      connection.Open();
      command.ExecuteNonQuery();

    }

    public bool InsertUser(User user)
    {
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        connection.Open();

        string query = "SELECT COUNT(*) FROM Users WHERE Email='" + user.Email + "';";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
          int count = (int)command.ExecuteScalar();
          if (count > 0)
          {
            connection.Close();
            return false;
          }
        }

        query = "INSERT INTO Users (FirstName, LastName, Address, Mobile, Email, Password, CreatedAt, ModifiedAt) VALUES (@fn, @ln, @add, @mb, @em, @pwd, @cat, @mat);";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
          command.Parameters.Add("@fn", SqlDbType.NVarChar).Value = user.FirstName;
          command.Parameters.Add("@ln", SqlDbType.NVarChar).Value = user.LastName;
          command.Parameters.Add("@add", SqlDbType.NVarChar).Value = user.Address;
          command.Parameters.Add("@mb", SqlDbType.NVarChar).Value = user.Mobile;
          command.Parameters.Add("@em", SqlDbType.NVarChar).Value = user.Email;
          command.Parameters.Add("@pwd", SqlDbType.NVarChar).Value = user.Password;
          command.Parameters.Add("@cat", SqlDbType.NVarChar).Value = user.CreatedAt;
          command.Parameters.Add("@mat", SqlDbType.NVarChar).Value = user.ModifiedAt;

          command.ExecuteNonQuery();
        }
      }
      return true;
    }

    public string IsUserPresent(string email, string password)
    {
      User user = new();
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        connection.Open();
        string query = "SELECT COUNT(*) FROM Users WHERE Email='" + email + "' AND Password='" + password + "';";
        command.CommandText = query;
        int count = (int)command.ExecuteScalar();
        if (count == 0)
        {
          connection.Close();
          return "";
        }

        query = "SELECT * FROM Users WHERE Email='" + email + "' AND Password='" + password + "';";
        command.CommandText = query;

        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          user.Id = (int)reader["UserId"];
          user.FirstName = (string)reader["FirstName"];
          user.LastName = (string)reader["LastName"];
          user.Email = (string)reader["Email"];
          user.Address = (string)reader["Address"];
          user.Mobile = (string)reader["Mobile"];
          user.Password = (string)reader["Password"];
          user.CreatedAt = (string)reader["CreatedAt"];
          user.ModifiedAt = (string)reader["ModifiedAt"];
        }

        string key = "aabbcc12345678901234567890aabbcc12345678901234567890aabbcc1234";
        string duration = "60";
        var symmetrickey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(symmetrickey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
          new Claim("id", user.Id.ToString()),
          new Claim("firstName", user.FirstName),
          new Claim("lastName", user.LastName),
          new Claim("address", user.Address),
          new Claim("mobile", user.Mobile),
          new Claim("email", user.Email),
          new Claim("createdAt", user.CreatedAt),
          new Claim("modifiedAt", user.ModifiedAt)
        };

        var jwtToken = new JwtSecurityToken(
          issuer: "localhost",
          audience: "localhost",
          claims: claims,
          expires: DateTime.Now.AddMinutes(Int32.Parse(duration)),
          signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
      }
      return "";
    }
    public bool RemoveProductFromCart(int productId, int cartItemId)
    {
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        connection.Open();
        string query = "DELETE FROM CartItems WHERE ProductId = @ProductId AND CartItemId = @CartItemId;";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@ProductId", productId);
          command.Parameters.AddWithValue("@CartItemId", cartItemId);
          int rowsAffected = command.ExecuteNonQuery();
          return rowsAffected > 0;
        }
      }
    }
    public bool UpdateProduct(Product product)
    {
      using (SqlConnection connection = new(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string query = @"UPDATE Products 
                         SET Title = @Title, Description = @Description, Price = @Price, 
                             Quantity = @Quantity, ImageName = @ImageName, CategoryId = @CategoryId, 
                             OfferId = @OfferId
                         WHERE ProductId = @ProductId;";

        command.CommandText = query;
        command.Parameters.Add("@Title", SqlDbType.NVarChar).Value = product.Title;
        command.Parameters.Add("@Description", SqlDbType.NVarChar).Value = product.Description;
        command.Parameters.Add("@Price", SqlDbType.Float).Value = product.Price;
        command.Parameters.Add("@Quantity", SqlDbType.Int).Value = product.Quantity;
        command.Parameters.Add("@ImageName", SqlDbType.NVarChar).Value = product.ImageName;
        command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = product.ProductCategory.Id;
        command.Parameters.Add("@OfferId", SqlDbType.Int).Value = product.Offer.Id;
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = product.Id;

        connection.Open();
        int rowsAffected = command.ExecuteNonQuery();
        return rowsAffected > 0;
      }
    }
    public List<Product> SearchProducts(string query)
    {
      var products = new List<Product>();
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        SqlCommand command = new()
        {
          Connection = connection
        };

        string sqlQuery = "SELECT * FROM Products WHERE Title LIKE '%' + @Query + '%';";
        command.CommandText = sqlQuery;
        command.Parameters.AddWithValue("@Query", query);

        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          var product = new Product()
          {
            Id = (int)reader["ProductId"],
            Title = (string)reader["Title"],
            Description = (string)reader["Description"],
            Price = (double)reader["Price"],
            Quantity = (int)reader["Quantity"],
            ImageName = (string)reader["ImageName"]
          };

          var categoryId = (int)reader["CategoryId"];
          product.ProductCategory = GetProductCategory(categoryId);

          var offerId = (int)reader["OfferId"];
          product.Offer = GetOffer(offerId);

          products.Add(product);
        }
      }
      return products;
    }
    public bool AddProduct(Product product)
    {
      if (product == null || product.Offer == null || product.Offer.Id <= 0)
      {
        return false;
      }
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        connection.Open();
        string offerQuery = "SELECT COUNT(*) FROM Offers WHERE OfferId = @OfferId;";
        using (SqlCommand offerCommand = new SqlCommand(offerQuery, connection))
        {
          offerCommand.Parameters.AddWithValue("@OfferId", product.Offer.Id);
          int offerCount = (int)offerCommand.ExecuteScalar();
          if (offerCount == 0)
          {
            return false;
          }
        }
        string query = "INSERT INTO Products (Title, Description, Price, Quantity, ImageName, CategoryId, OfferId) " +
                       "VALUES (@Title, @Description, @Price, @Quantity, @ImageName, @CategoryId, @OfferId);";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@Title", product.Title);
          command.Parameters.AddWithValue("@Description", product.Description);
          command.Parameters.AddWithValue("@Price", product.Price);
          command.Parameters.AddWithValue("@Quantity", product.Quantity);
          command.Parameters.AddWithValue("@ImageName", product.ImageName);
          command.Parameters.AddWithValue("@CategoryId", product.ProductCategory.Id);
          command.Parameters.AddWithValue("@OfferId", product.Offer.Id);

          int rowsAffected = command.ExecuteNonQuery();

          return rowsAffected > 0;
        }
      }
    }
    public void InsertMessage(string message)
    {
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        SqlCommand command = new SqlCommand
        {
          Connection = connection
        };

        string query = "INSERT INTO Messages (Info, CreatedOn) VALUES (@Info, @CreatedOn);";
        command.CommandText = query;
        command.Parameters.AddWithValue("@Info", message);
        command.Parameters.AddWithValue("@CreatedOn", DateTime.Now); 

        connection.Open();
        command.ExecuteNonQuery();
      }
    }
    public bool UpdateProductQuantity(int productId, int quantity = -1)
    {
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        connection.Open();

        // בודק אם המוצר קיים
        string Query = "SELECT COUNT(*) FROM Products WHERE ProductId = @ProductId;";
        using (SqlCommand Command = new SqlCommand(Query, connection))
        {
          Command.Parameters.AddWithValue("@ProductId", productId);
          int productCount = (int)Command.ExecuteScalar();
          if (productCount == 0)
          {
            return false; // מוצר לא נמצא
          }
        }

        // מעדכן כמות מוצר
        string query = "UPDATE Products SET Quantity = @Quantity WHERE ProductId = @ProductId;";
        using (SqlCommand Command = new SqlCommand(query, connection))
        {
          Command.Parameters.AddWithValue("@Quantity", quantity);
          Command.Parameters.AddWithValue("@ProductId", productId);

          int rowsAffected = Command.ExecuteNonQuery();
          return rowsAffected > 0; // אם בוצע עדכון
        }
      }
    }
    public bool DeleteProduct(int productId)
    {
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        connection.Open();

        // בודק אם המוצר קיים
        string query = "SELECT COUNT(*) FROM Products WHERE ProductId = @ProductId;";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@ProductId", productId);
          int productCount = (int)command.ExecuteScalar();
          if (productCount == 0)
          {
            return false;
          }
        }

        // מוחק
        string deleteQuery = "DELETE FROM Products WHERE ProductId = @ProductId;";
        using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
        {
          deleteCommand.Parameters.AddWithValue("@ProductId", productId);
          int rowsAffected = deleteCommand.ExecuteNonQuery();
          return rowsAffected > 0; 
        }
      }
    }
    public bool AddProductImage(int productId, string imageUrl)
    {
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        connection.Open();
        string query = "INSERT INTO ProductPhotos (ProductId, ImageUrl) VALUES (@ProductId, @ImageUrl);";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
          command.Parameters.AddWithValue("@ProductId", productId);
          command.Parameters.AddWithValue("@ImageUrl", imageUrl);
          int rowsAffected = command.ExecuteNonQuery();
          return rowsAffected > 0;
        }
      }
    }
    public List<Order> GetAllOrders()
    {
      List<Order> orders = new List<Order>();
      using (SqlConnection connection = new SqlConnection(dbconnection))
      {
        SqlCommand command = new SqlCommand();
        command.Connection = connection;
        connection.Open();

        string query = "SELECT Id, UserId, CartId FROM Orders;";
        command.CommandText = query;

        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          Order order = new Order();
          order.Id = (int)reader["Id"];
          order.User.Id = (int)reader["UserId"];
          order.Cart.Id = (int)reader["CartId"];
          orders.Add(order);
        }
      }
      return orders;
    }
  }

}
