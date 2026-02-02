using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;

namespace OsuScoreStats.Controllers;

public class UserController(IDbContextFactory<ScoreDataContext> dbContextFactory) : Controller
{
    public async Task<IActionResult> Index(int id, CancellationToken cancellationToken = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var userRepository = new UserRepository(dbContext);

        var user = await userRepository.GetAsync(id, cancellationToken);
        
        if (user == null)
            return NotFound();
        
        return View(user);
    }
}