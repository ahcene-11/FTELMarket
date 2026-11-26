using FTELMarket.Data;
using FTELMarket.DTOs;
using Microsoft.EntityFrameworkCore;
using FTELMarket.Models;
using Azure;

namespace FTELMarket.Services;

public class FlakyService(HttpClient client, FtelMarketDbContext context, ILogger<FlakyService> logger)
{
    public async Task<List<ExternalCustomerDto>> GetAllCustomersAsync()
    {
    var customers = new List<ExternalCustomerDto>();

    int page = 1;
    int pageSize = 50;
    int total;

    do
    {
        var response = await client.GetFromJsonAsync<PagedResponse<ExternalCustomerDto>>(
            $"api/customers?page={page}&pageSize={pageSize}");

        if (response == null)
        {
            throw new InvalidOperationException("Réponse vide de Flaky API.");
        }

        customers.AddRange(response.Items);

        total = response.Total;
        page++;

    } while (customers.Count < total);

    return customers;
}

public async Task SynchronizeCustomersAsync()
{
    try{
    logger.LogInformation("Démarrage de la synchronisation des clients avec l'API externe.");
    // Récupérer tous les clients depuis l'API externe
    var externalCustomers = await GetAllCustomersAsync();

    logger.LogInformation(
    "{Count} clients récupérés depuis l'API externe.",
    externalCustomers.Count);

    // Récupérer les clients déjà présents dans notre base
    var localCustomers = await context.Customers.ToListAsync();

    logger.LogInformation(
    "{Count} clients présents dans la base locale.",
    localCustomers.Count);
    
    //dictionnaire pour faciliter la comparaison
    var externalCustomersById = externalCustomers.ToDictionary(c => c.Id);
    var localCustomersById = localCustomers.ToDictionary(c => c.Id);

    int created = 0;
    int updated = 0;
    int deleted = 0;

    foreach (var externalCustomer in externalCustomers)
    {
    if (localCustomersById.TryGetValue(externalCustomer.Id, out var localCustomer))
    {
        // Le client existe déjà : on met à jour ses informations
        localCustomer.FirstName = externalCustomer.FirstName;
        localCustomer.LastName = externalCustomer.LastName;
        localCustomer.Email = externalCustomer.Email;
        localCustomer.City = externalCustomer.City;
        updated++;
    }
    else
    {
        // Le client n'existe pas : on le crée
        var newCustomer = new Customer
        {
            Id = externalCustomer.Id,
            FirstName = externalCustomer.FirstName,
            LastName = externalCustomer.LastName,
            Email = externalCustomer.Email,
            City = externalCustomer.City
        };

        context.Customers.Add(newCustomer);
        created++;
    }
    }
    //suppression des clients locaux qui ne sont plus présents dans l'API externe
    foreach (var localCustomer in localCustomers)
    {
        if (!externalCustomersById.ContainsKey(localCustomer.Id))
        {
        context.Customers.Remove(localCustomer);
        deleted++;
        }
    }
    // Enregistrer les changements dans la base de données
    await context.SaveChangesAsync();
    logger.LogInformation(
        "Synchronisation terminée. Créés : {Created}, Mis à jour : {Updated}, Supprimés : {Deleted}.",
        created, updated, deleted);
    logger.LogInformation("Synchronisation des clients avec l'API externe terminée.");

    }catch(Exception ex)
        {
            logger.LogError(ex, "Une erreur est survenue lors de la synchronisation des clients avec l'API externe.");
            throw;
        }
}

    public async Task<List<ExternalProductDto>> GetAllProductsAsync()
    {
        // Récupérer tous les produits depuis l'API externe
        var products = new List<ExternalProductDto>();

        int page = 1;
        int pageSize = 50;
        int total;

        do
        {
            var response = await client.GetFromJsonAsync<PagedResponse<ExternalProductDto>>(
                $"api/products?page={page}&pageSize={pageSize}");

                if(response == null)
                {
                    throw new InvalidOperationException("Réponse vide de Flaky API.");
                }

                products.AddRange(response.Items);

                total = response.Total;
                page++;

            
        } while (products.Count < total);

        return products;
    }

    public async Task SynchronizeProductsAsync()
    {
        try
        {
        logger.LogInformation("Démarrage de la synchronisation des produits avec l'API externe.");

        var externalProducts = await GetAllProductsAsync();

        logger.LogInformation(
            "{Count} produits récuperés depuis l'API externe. ",
            externalProducts.Count);
        
        var localProducts = await context.Products
            .Include(p => p.Category)
            .ToListAsync();
        
        logger.LogInformation(
            "{Count} produits présents dans la base locale. ",
            localProducts.Count);

        //dictionnaire pour faciliter la comparaison
        var externalProductsById = externalProducts.ToDictionary(p => p.Id);

        var localProductsByExternalId = localProducts
             .Where(p => p.ExternalId.HasValue)
             .ToDictionary(p => p.ExternalId!.Value);
        

        //  Récupération des catégories
        var categories = await context.Categories.ToListAsync();

        var categoriesByName = categories.ToDictionary(
            c => c.Name,
            StringComparer.OrdinalIgnoreCase);
        
        int created = 0;
        int updated = 0;
        int deleted = 0;

        // Création et mise a jour des produits
        foreach (var externalProduct in externalProducts)
        {
            //recherche ou creation de la categorie
            if(!categoriesByName.TryGetValue(externalProduct.Category, out var category))
            {
                category = new Category
                {
                    Name = externalProduct.Category
                };
                context.Categories.Add(category);
                categoriesByName.Add(category.Name, category);
            }
            // recherche du produit local 
            if (localProductsByExternalId.TryGetValue(
                externalProduct.Id, out var localProduct))
            {
                //mise a jour
                localProduct.Name = externalProduct.Name;
                localProduct.Reference = externalProduct.Reference;
                localProduct.Price = externalProduct.Price;
                localProduct.StockQuantity = externalProduct.Stock;
                localProduct.Category = category;

                updated++;
            }
            else
            {
                //creation du produit
                var newProduct = new Product
                {
                    ExternalId = externalProduct.Id,
                    Name = externalProduct.Name,
                    Reference = externalProduct.Reference,
                    Price = externalProduct.Price,
                    StockQuantity = externalProduct.Stock,
                    Category = category,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                context.Products.Add(newProduct);
                created++;
            }
        }
        // Suppression des produits absents de l'api
        foreach(var localProduct in localProducts)
        {
            if(!localProduct.ExternalId.HasValue &&
                !externalProductsById.ContainsKey(localProduct.ExternalId.Value))
            {
                context.Products.Remove(localProduct);
                deleted++;
            }
        }
        // Enregistrer les changements dans la base de données
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Synchronisation terminée. Créés : {Created}, Mis à jour : {Updated}, Supprimés : {Deleted}.",
            created, updated, deleted);

        logger.LogInformation("Synchronisation des produits avec l'API externe terminée.");
    }
    catch(Exception ex)
        {
            logger.LogError(ex, "Une erreur est survenue lors de la synchronisation des produits avec l'API externe.");
            throw;
        }
    }

}



