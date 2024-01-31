using EFTesting.Extensions;
using EFTesting.SqlServer;
using Microsoft.EntityFrameworkCore;


var rates = new[] { "na", "ce" };

using var context = new DataContext();

var works = await context.Achievements
    .Include(x => x.User)
    .Where(x => rates.Any(w => EF.Functions.Like(x.User.Email, "%" + w + "%")))
    .ToListAsync();

var notWork = await context.Achievements
    .Include(x => x.User)
    .Like(x => x.User.Email, rates)
    .ToListAsync();

Console.ReadLine();
