using FTELMarket.Data;
using FTELMarket.DTOs;
using Microsoft.EntityFrameworkCore;
using FTELMarket.Models;

namespace FTELMarket.Services;

public class FlakyService(HttpClient client, FtelMarketDbContext context)
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
    // Récupérer tous les clients depuis l'API externe
    var externalCustomers = await GetAllCustomersAsync();

    // Récupérer les clients déjà présents dans notre base
    var localCustomers = await context.Customers.ToListAsync();
    
    //dictionnaire pour faciliter la comparaison
    var externalCustomersById = externalCustomers.ToDictionary(c => c.Id);
    var localCustomersById = localCustomers.ToDictionary(c => c.Id);

    foreach (var externalCustomer in externalCustomers)
    {
    if (localCustomersById.TryGetValue(externalCustomer.Id, out var localCustomer))
    {
        // Le client existe déjà : on met à jour ses informations
        localCustomer.FirstName = externalCustomer.FirstName;
        localCustomer.LastName = externalCustomer.LastName;
        localCustomer.Email = externalCustomer.Email;
        localCustomer.City = externalCustomer.City;
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
    }
    }
    //suppression des clients locaux qui ne sont plus présents dans l'API externe
    foreach (var localCustomer in localCustomers)
    {
        if (!externalCustomersById.ContainsKey(localCustomer.Id))
        {
        context.Customers.Remove(localCustomer);
        }
    }
    // Enregistrer les changements dans la base de données
    await context.SaveChangesAsync();
}

}



