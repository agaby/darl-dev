using Darl.GraphQL.Models.Models;
using GraphQL;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darl.GraphQL.Models.Connectivity
{

    /// <summary>
    /// makes Stripe single source of truth for products
    /// download the current set of active products
    /// </summary>
    public class Products : IProducts
    {
        public List<DarlProduct> products { get; } = new List<DarlProduct>();
        private IConfiguration _config;
        public Products(IConfiguration config)
        {
            _config = config;
            var sak = _config["AppSettings:StripeAPIKey"];
            try
            {
                if (String.IsNullOrEmpty(sak))
                    throw new ExecutionError("Subscriptions not enabled");
                StripeConfiguration.ApiKey = sak;
                var prods = new ProductService();
                var prices = new PriceService();
                foreach(var p in prods.List(new ProductListOptions { Active = true}))
                {
                    var price = prices.List(new PriceListOptions { Active = true, Product = p.Id }).FirstOrDefault();
                    if (price != null)
                    {
                        products.Add(new DarlProduct
                        {
                            id = p.Id,
                            currency = price.Currency,
                            description = p.Description,
                            priceId = price.Id,
                            image = p.Images.FirstOrDefault(),
                            name = p.Name,
                            price = price.UnitAmount ?? 0l,
                            hidden = bool.Parse(p.Metadata["Hidden"]),
                            kgCount = int.Parse(p.Metadata["KGCount"]),
                            userCount = int.Parse(p.Metadata["UserCount"])
                        });
                    }
                }
                products = products.OrderBy(a => a.price).ToList();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
