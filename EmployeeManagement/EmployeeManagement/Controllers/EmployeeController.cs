using System;
using System.Web.Mvc;
using EmployeeManagement.DataAccess;
using EmployeeManagement.Models;

namespace EmployeeManagement.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeRepository _repository;
        public EmployeeController()
        {
            _repository = new EmployeeRepository();
        }

        public ActionResult Index()
        {
            var employees = _repository.GetAllEmployees();
            return View(employees);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _repository.AddEmployee(employee);
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        public ActionResult Edit(int id)
        {
            var employee = _repository.GetEmployeeById(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _repository.UpdateEmployee(employee);
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        public ActionResult Delete(int id)
        {
            var employee = _repository.GetEmployeeById(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteEmployee(id);
            return RedirectToAction("Index");
        }
    }
}