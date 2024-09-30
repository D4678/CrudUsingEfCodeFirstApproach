using CrudOperation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeContext _dbContext;

        public EmployeeController(EmployeeContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>>GetEmployee()
        {
            if(_dbContext.Employees == null)
            {
                return NotFound();
            }
            return await _dbContext.Employees.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>>GetEmployee(int id)
        {
            if (_dbContext.Employees == null)
            {
                return NotFound();
            }
            var employee = await _dbContext.Employees.FindAsync(id); 
            if(employee==null)
            {
                return NotFound(id);
            }
            return employee;
        }

        [HttpPost]
        public async Task<ActionResult<Employee>>CreateEmp(Employee employee)
        {
            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new {id = employee.ID}, employee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee updatedEmployee)
        {
            // Check if the ID in the URL matches the employee object
            if (id != updatedEmployee.ID)
            {
                return BadRequest("Employee ID mismatch.");
            }

            // Check if the employee exists in the database
            var employee = await _dbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound($"Employee with ID {id} not found.");
            }

            // Update the employee fields
            employee.FirstName = updatedEmployee.FirstName;
            employee.LastName = updatedEmployee.LastName;
            employee.Gender = updatedEmployee.Gender;
            employee.Salary = updatedEmployee.Salary;

            // Save changes to the database
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeAvailable(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content indicates the update was successful
        }
        private bool EmployeeAvailable(int id)
        {
            return _dbContext.Employees.Any(e => e.ID == id);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Employee>> DeleteEmp(int id)
        {
            if(_dbContext.Employees == null)
            {
                return NotFound();
            }

            var employee = await _dbContext.Employees.FindAsync(id);
            if(employee == null)
            {
                return NotFound();
            }

            _dbContext.Employees.Remove(employee);
            
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
