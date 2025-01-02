using SwenProject_Arslan.Models;

namespace SwenProject_Arslan.Repositories;

public class UserService
{
    private readonly DbHandler _dbHandler;

    public UserService(DbHandler dbHandler)
    {
        _dbHandler = dbHandler;
    }

    public async Task CreateUserAsync(User user)
    {
        await DbHandler.InsertAsync(user);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _dbHandler.GetAllAsync<User>();
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _dbHandler.GetByIdAsync<User>(id);
    }

    public async Task UpdateUserAsync(User user, int id)
    {
        await _dbHandler.UpdateAsync(user, id);
    }

    public async Task DeleteUserAsync(int id)
    {
        await _dbHandler.DeleteAsync<User>(id);
    }
}
