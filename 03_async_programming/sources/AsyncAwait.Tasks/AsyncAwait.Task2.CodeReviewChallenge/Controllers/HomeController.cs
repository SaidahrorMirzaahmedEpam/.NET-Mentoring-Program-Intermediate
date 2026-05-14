using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AsyncAwait.Task2.CodeReviewChallenge.Models;
using AsyncAwait.Task2.CodeReviewChallenge.Models.Support;
using AsyncAwait.Task2.CodeReviewChallenge.Services;
using CloudServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AsyncAwait.Task2.CodeReviewChallenge.Controllers;

public class HomeController : Controller
{
    private readonly IAssistant _assistant;

    private readonly IPrivacyDataService _privacyDataService;

    private readonly IStatisticService _statisticService;

    public HomeController(IAssistant assistant, IPrivacyDataService privacyDataService, IStatisticService statisticService)
    {
        _assistant = assistant ?? throw new ArgumentNullException(nameof(assistant));
        _privacyDataService = privacyDataService ?? throw new ArgumentNullException(nameof(privacyDataService));
        _statisticService = statisticService ?? throw new ArgumentNullException(nameof(statisticService));
    }

    public async Task<ActionResult> Index()
    {
        var count = await _statisticService.GetVisitsCountAsync(Request.Path).ConfigureAwait(false);
        ViewBag.TotalPageVisits = (count +1).ToString();
        return View();
    }

    public async Task<ActionResult> Privacy()
    {
        ViewBag.Message = await _privacyDataService.GetPrivacyDataAsync().ConfigureAwait(false);
        var count = await _statisticService.GetVisitsCountAsync(Request.Path).ConfigureAwait(false);
        ViewBag.TotalPageVisits = (count +1).ToString();
        return View();
    }

    public async Task<IActionResult> Help()
    {
        ViewBag.RequestInfo = await _assistant.RequestAssistanceAsync("guest").ConfigureAwait(false);
        var count = await _statisticService.GetVisitsCountAsync(Request.Path).ConfigureAwait(false);
        ViewBag.TotalPageVisits = (count +1).ToString();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}
