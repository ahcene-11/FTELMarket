using FTELMarket.Data;
using FTELMarket.DTOs;
using Microsoft.EntityFrameworkCore;
using FTELMarket.Models;

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

}



