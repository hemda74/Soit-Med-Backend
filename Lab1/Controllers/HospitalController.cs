using Lab1.DTO;
using Lab1.Models;
using Lab1.Models.Hospital;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalController : ControllerBase
    {
        private readonly Context context;

        public HospitalController(Context _context)
        {
            context = _context;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetHospitals()
        {
            var hospitals = await context.Hospitals
                .Select(h => new HospitalResponseDTO
                {
                    HospitalId = h.HospitalId,
                    Name = h.Name,
                    Location = h.Location,
                    Address = h.Address,
                    PhoneNumber = h.PhoneNumber,
                    CreatedAt = h.CreatedAt,
                    IsActive = h.IsActive,
                    DoctorCount = h.Doctors.Count(),
                    TechnicianCount = h.Technicians.Count()
                })
                .ToListAsync();

            return Ok(hospitals);
        }

        [HttpGet("{hospitalId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetHospital(string hospitalId)
        {
            var hospital = await context.Hospitals
                .Include(h => h.Doctors)
                .Include(h => h.Technicians)
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId);

            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var response = new HospitalResponseDTO
            {
                HospitalId = hospital.HospitalId,
                Name = hospital.Name,
                Location = hospital.Location,
                Address = hospital.Address,
                PhoneNumber = hospital.PhoneNumber,
                CreatedAt = hospital.CreatedAt,
                IsActive = hospital.IsActive,
                DoctorCount = hospital.Doctors.Count(),
                TechnicianCount = hospital.Technicians.Count()
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateHospital(HospitalDTO hospitalDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if hospital ID already exists
            if (await context.Hospitals.AnyAsync(h => h.HospitalId == hospitalDTO.HospitalId))
            {
                return BadRequest($"Hospital with ID '{hospitalDTO.HospitalId}' already exists");
            }

            var hospital = new Hospital
            {
                HospitalId = hospitalDTO.HospitalId,
                Name = hospitalDTO.Name,
                Location = hospitalDTO.Location,
                Address = hospitalDTO.Address,
                PhoneNumber = hospitalDTO.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Hospitals.Add(hospital);
            await context.SaveChangesAsync();

            return Ok($"Hospital '{hospital.Name}' created successfully with ID: {hospital.HospitalId}");
        }

        [HttpPut("{hospitalId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateHospital(string hospitalId, HospitalDTO hospitalDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hospital = await context.Hospitals.FindAsync(hospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            hospital.Name = hospitalDTO.Name;
            hospital.Location = hospitalDTO.Location;
            hospital.Address = hospitalDTO.Address;
            hospital.PhoneNumber = hospitalDTO.PhoneNumber;

            await context.SaveChangesAsync();

            return Ok($"Hospital '{hospital.Name}' updated successfully");
        }

        [HttpDelete("{hospitalId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteHospital(string hospitalId)
        {
            var hospital = await context.Hospitals
                .Include(h => h.Doctors)
                .Include(h => h.Technicians)
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId);

            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            if (hospital.Doctors.Any() || hospital.Technicians.Any())
            {
                return BadRequest($"Cannot delete hospital '{hospital.Name}' because it has {hospital.Doctors.Count()} doctors and {hospital.Technicians.Count()} technicians assigned to it");
            }

            context.Hospitals.Remove(hospital);
            await context.SaveChangesAsync();

            return Ok($"Hospital '{hospital.Name}' deleted successfully");
        }

        // Doctor management endpoints
        [HttpPost("{hospitalId}/doctors")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AddDoctor(string hospitalId, DoctorDTO doctorDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hospital = await context.Hospitals.FindAsync(hospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var doctor = new Doctor
            {
                Name = doctorDTO.Name,
                Specialty = doctorDTO.Specialty,
                HospitalId = hospitalId,
                UserId = doctorDTO.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();

            return Ok($"Doctor '{doctor.Name}' added to hospital '{hospital.Name}' successfully");
        }

        [HttpGet("{hospitalId}/doctors")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetHospitalDoctors(string hospitalId)
        {
            var hospital = await context.Hospitals
                .Include(h => h.Doctors)
                .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId);

            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var doctors = hospital.Doctors.Select(d => new
            {
                d.DoctorId,
                d.Name,
                d.Specialty,
                d.IsActive,
                d.CreatedAt,
                User = d.User != null ? new { d.User.UserName, d.User.Email } : null
            });

            return Ok(new
            {
                Hospital = hospital.Name,
                DoctorCount = doctors.Count(),
                Doctors = doctors
            });
        }

        // Technician management endpoints
        [HttpPost("{hospitalId}/technicians")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AddTechnician(string hospitalId, TechnicianDTO technicianDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hospital = await context.Hospitals.FindAsync(hospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var technician = new Technician
            {
                Name = technicianDTO.Name,
                Department = technicianDTO.Department,
                HospitalId = hospitalId,
                UserId = technicianDTO.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Technicians.Add(technician);
            await context.SaveChangesAsync();

            return Ok($"Technician '{technician.Name}' added to hospital '{hospital.Name}' successfully");
        }

        [HttpGet("{hospitalId}/technicians")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetHospitalTechnicians(string hospitalId)
        {
            var hospital = await context.Hospitals
                .Include(h => h.Technicians)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId);

            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var technicians = hospital.Technicians.Select(t => new
            {
                t.TechnicianId,
                t.Name,
                t.Department,
                t.IsActive,
                t.CreatedAt,
                User = t.User != null ? new { t.User.UserName, t.User.Email } : null
            });

            return Ok(new
            {
                Hospital = hospital.Name,
                TechnicianCount = technicians.Count(),
                Technicians = technicians
            });
        }
    }
}
