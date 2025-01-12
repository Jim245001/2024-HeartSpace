﻿using Microsoft.AspNetCore.Mvc;
using HeartSpaceAdmin.Models.EFModels;
using System.Linq;
using HeartSpaceAdmin.Models;

public class MemberController : Controller
{
	private readonly AppDbContext _context;

	public MemberController(AppDbContext context)
	{
		_context = context;
	}

	// Index
	public IActionResult Index()
	{
		var members = _context.Members.Select(m => new MemberViewModel
		{
			Id = m.Id,
			Account = m.Account,
			Name = m.Name,
			NickName = m.NickName,
			Email = m.Email,
			Disabled = m.Disabled,
			Role = m.Role,
			AbsenceCount = m.AbsenceCount,
			MemberImg = m.MemberImg
		}).ToList();

		return View(members);
	}

	// Create (GET)
	public IActionResult Create()
	{
		return View(new MemberViewModel());
	}

	// Create (POST)
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Create(MemberViewModel model)
	{
		if (ModelState.IsValid)
		{
			var member = new Member
			{
				Account = model.Account,
				Name = model.Name,
				NickName = model.NickName,
				Email = model.Email,
				Disabled = model.Disabled,
				Role = model.Role,
				AbsenceCount = model.AbsenceCount,
				MemberImg = model.MemberImg,
				PasswordHash = model.PasswordHash,
				ConfirmCode = model.ConfirmCode,
				IsConfirmed = model.IsConfirmed
			};

			_context.Members.Add(member);
			_context.SaveChanges();
			return RedirectToAction(nameof(Index));
		}

		return View(model);
	}

	// Edit (GET)
	public IActionResult Edit(int id)
	{
		var member = _context.Members.Find(id);
		if (member == null)
		{
			return NotFound();
		}

		var model = new MemberViewModel
		{
			Id = member.Id,
			Account = member.Account,
			Name = member.Name,
			NickName = member.NickName,
			Email = member.Email,
			Disabled = member.Disabled,
			Role = member.Role,
			AbsenceCount = member.AbsenceCount,
			MemberImg = member.MemberImg,
			PasswordHash = member.PasswordHash,
			ConfirmCode = member.ConfirmCode,
			IsConfirmed = member.IsConfirmed
		};

		return View(model);
	}

	// Edit (POST)
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Edit(MemberViewModel model)
	{
		if (ModelState.IsValid)
		{
			var member = _context.Members.Find(model.Id);
			if (member == null)
			{
				return NotFound();
			}

			member.Account = model.Account;
			member.Name = model.Name;
			member.NickName = model.NickName;
			member.Email = model.Email;
			member.Disabled = model.Disabled;
			member.Role = model.Role;
			member.AbsenceCount = model.AbsenceCount;
			member.MemberImg = model.MemberImg;
			member.PasswordHash = model.PasswordHash;
			member.ConfirmCode = model.ConfirmCode;
			member.IsConfirmed = model.IsConfirmed;

			_context.SaveChanges();
			return RedirectToAction(nameof(Index));
		}

		return View(model);
	}

	// Disable (POST)
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Disable(int id)
	{
		var member = _context.Members.Find(id);
		if (member == null)
		{
			return NotFound();
		}

		member.Disabled = true;
		_context.SaveChanges();
		return RedirectToAction(nameof(Index));
	}
}
