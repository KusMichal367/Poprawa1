namespace Poprawa1.Services;
using System.Data.Common;
using Poprawa1.Models;
using Microsoft.Data.SqlClient;
public class RentalService : IRentalService
{
    private readonly String _connectionString;

    public RentalService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<ClientDTO> GetClientAsync(int id)
    {
        string sqlQuery =
            @"Select C.ID as ClientId, C.FirstName as ClientName, C.LastName as ClientLastName, C.Address as ClientAddress, Car.VIN as VIN, Cl.Name as Color, M.Name as Model, cr.DateFrom as DateFrom, cr.DateTo as DateTo, Cr.TotalPrice as TotalPrice From clients C join car_rentals CR on C.ID = CR.ClientID join cars Car on CR.CarID = Car.ID join colors Cl on Car.ColorID = Cl.ID join models M on Car.ModelID = M.ID where C.ID = @id;";

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand(sqlQuery, connection);

        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        ClientDTO? clientDto = null;

        while (await reader.ReadAsync())
        {
            if (clientDto is null)
            {
                clientDto = new ClientDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ClientId")),
                    FirstName = reader.GetString(reader.GetOrdinal("ClientName")),
                    LastName = reader.GetString(reader.GetOrdinal("ClientLastName")),
                    Adress = reader.GetString(reader.GetOrdinal("ClientAddress"))
                };
            }

            clientDto.Rentals.Add(new RentalDTO
            {
               VIN = reader.GetString(reader.GetOrdinal("VIN")),
               Color = reader.GetString(reader.GetOrdinal("Color")),
               Model = reader.GetString(reader.GetOrdinal("Model")),
               DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
               DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
               TotalPrice = reader.GetInt32(reader.GetOrdinal("TotalPrice"))
            });
        }

        if (clientDto is null)
        {
            throw new ApplicationException($"Could not find client with id {id}");
        }

        return clientDto;
    }

    public async Task AddClientAndRentalAsync(InputDTO input)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            //1 czy samochód istnieje
            command.Parameters.Clear();
            command.CommandText = "Select count(*) from cars where ID = @CarID;";
            command.Parameters.AddWithValue("@CarID", input.CarId);

            int bookingWithId = (int) await command.ExecuteScalarAsync();
            if (bookingWithId == 0)
            {
                throw new InvalidOperationException($"Could not find car with id {input.CarId}");
            }

            //2 totaldays

            int days = (input.DateTo - input.DateFrom).Days;
            Console.WriteLine(days);

            //3 wstawianie klienta
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO clients VALUES (@FirstName, @LastName, @Adress)";

            command.Parameters.AddWithValue("@FirstName", input.Client.FirstName);
            command.Parameters.AddWithValue("@LastName", input.Client.LastName);
            command.Parameters.AddWithValue("@Adress", input.Client.Adress);

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add client: {ex.Message}");
            }

            //4 price per day
            command.Parameters.Clear();
            command.CommandText = "SELECT PricePerDay From cars where ID=@CarID;";
            command.Parameters.AddWithValue("@CarID", input.CarId);

            int pricePerDay = (int) await command.ExecuteScalarAsync();

            //5 id of client
            command.Parameters.Clear();
            command.CommandText =
                "SELECT ID From clients where FirstName = @ClientName and LastName = @ClientLastName and Address = @@ClientAddress;";
            command.Parameters.AddWithValue("@ClientName", input.Client.FirstName);
            command.Parameters.AddWithValue("@ClientLastName", input.Client.LastName);
            command.Parameters.AddWithValue("@ClientAddress", input.Client.Adress);

            int clientId = (int) await command.ExecuteScalarAsync();

            //6 insert
            command.Parameters.Clear();
            command.CommandText =
                "INSERT INTO car_rentals VALUES (@ClientId, @CarId, @DateFrom, @DateTO, @TotalPrice, null)";
            command.Parameters.AddWithValue("@ClientId", clientId);
            command.Parameters.AddWithValue("@CarId", input.CarId);
            command.Parameters.AddWithValue("@DateFrom", input.DateFrom);
            command.Parameters.AddWithValue("@DateTo", input.DateTo);
            command.Parameters.AddWithValue("@TotalPrice", pricePerDay*days);

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add client: {ex.Message}");
            }

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}