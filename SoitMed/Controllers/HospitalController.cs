using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Hospital;
using SoitMed.Repositories;
using SoitMed.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public HospitalController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize]
        [CaseInsensitiveRoleAuthorization("SuperAdmin", "Admin")]
        public async Task<IActionResult> GetHospitals()
        {
            var hospitals = await _unitOfWork.Hospitals.GetActiveHospitalsAsync();
            
            var response = hospitals.Select(h => new HospitalResponseDTO
            {
                HospitalId = h.HospitalId,
                Name = h.Name,
                Location = h.Location,
                Address = h.Address,
                PhoneNumber = h.PhoneNumber,
                CreatedAt = h.CreatedAt,
                IsActive = h.IsActive,
                DoctorCount = h.DoctorHospitals.Count(dh => dh.IsActive),
                TechnicianCount = h.Technicians.Count()
            });

            return Ok(response);
        }

        [HttpGet("{hospitalId}")]
        [Authorize]
        [CaseInsensitiveRoleAuthorization("SuperAdmin", "Admin")]
        public async Task<IActionResult> GetHospital(string hospitalId)
        {
            var hospital = await _unitOfWork.Hospitals.GetHospitalWithAllDetailsAsync(hospitalId);

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
                DoctorCount = hospital.DoctorHospitals.Count(dh => dh.IsActive),
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
            if (await _unitOfWork.Hospitals.ExistsByHospitalIdAsync(hospitalDTO.HospitalId))
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

            await _unitOfWork.Hospitals.CreateAsync(hospital);
            await _unitOfWork.SaveChangesAsync();

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

            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(hospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            hospital.Name = hospitalDTO.Name;
            hospital.Location = hospitalDTO.Location;
            hospital.Address = hospitalDTO.Address;
            hospital.PhoneNumber = hospitalDTO.PhoneNumber;

            await _unitOfWork.Hospitals.UpdateAsync(hospital);
            await _unitOfWork.SaveChangesAsync();

            return Ok($"Hospital '{hospital.Name}' updated successfully");
        }

        [HttpDelete("{hospitalId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteHospital(string hospitalId)
        {
            var hospital = await _unitOfWork.Hospitals.GetHospitalWithAllDetailsAsync(hospitalId);

            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            if (hospital.DoctorHospitals.Any(dh => dh.IsActive) || hospital.Technicians.Any())
            {
                return BadRequest($"Cannot delete hospital '{hospital.Name}' because it has {hospital.DoctorHospitals.Count(dh => dh.IsActive)} Doctors and {hospital.Technicians.Count()} Technicians assigned to it");
            }

            await _unitOfWork.Hospitals.DeleteAsync(hospital);
            await _unitOfWork.SaveChangesAsync();

            return Ok($"Hospital '{hospital.Name}' deleted successfully");
        }

        // Doctor management endpoints
        [HttpPost("{hospitalId}/Doctors")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AddDoctor(string hospitalId, DoctorDTO DoctorDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(hospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var Doctor = new Doctor
            {
                Name = DoctorDTO.Name,
                Specialty = DoctorDTO.Specialty,
                UserId = DoctorDTO.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Doctors.CreateAsync(Doctor);
            await _unitOfWork.SaveChangesAsync();

            // Create the many-to-many relationship
            var DoctorHospital = new DoctorHospital
            {
                DoctorId = Doctor.DoctorId,
                HospitalId = hospitalId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.DoctorHospitals.CreateAsync(DoctorHospital);
            await _unitOfWork.SaveChangesAsync();

            return Ok($"Doctor '{Doctor.Name}' added to hospital '{hospital.Name}' successfully");
        }

        [HttpGet("{hospitalId}/Doctors")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetHospitalDoctors(string hospitalId)
        {
            var hospital = await _unitOfWork.Hospitals.GetHospitalWithDoctorsAsync(hospitalId);

            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var Doctors = hospital.DoctorHospitals
                .Where(dh => dh.IsActive)
                .Select(dh => new
                {
                    dh.Doctor.DoctorId,
                    dh.Doctor.Name,
                    dh.Doctor.Specialty,
                    dh.Doctor.IsActive,
                    dh.Doctor.CreatedAt,
                    dh.AssignedAt,
                    User = dh.Doctor.User != null ? new { dh.Doctor.User.UserName, dh.Doctor.User.Email } : null
                });

            return Ok(new
            {
                Hospital = hospital.Name,
                DoctorCount = Doctors.Count(),
                Doctors = Doctors
            });
        }

        // Many-to-many relationship management endpoints
        [HttpPost("{hospitalId}/Doctors/{DoctorId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AssignDoctorToHospital(string hospitalId, int DoctorId)
        {
            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(hospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var Doctor = await _unitOfWork.Doctors.GetByIdAsync(DoctorId);
            if (Doctor == null)
            {
                return NotFound($"Doctor with ID {DoctorId} not found");
            }

            // Check if already assigned
            if (await _unitOfWork.Doctors.IsDoctorAssignedToHospitalAsync(DoctorId, hospitalId))
            {
                return BadRequest($"Doctor '{Doctor.Name}' is already assigned to hospital '{hospital.Name}'");
            }

            var DoctorHospital = new DoctorHospital
            {
                DoctorId = DoctorId,
                HospitalId = hospitalId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.DoctorHospitals.CreateAsync(DoctorHospital);
            await _unitOfWork.SaveChangesAsync();

            return Ok($"Doctor '{Doctor.Name}' assigned to hospital '{hospital.Name}' successfully");
        }

        [HttpDelete("{hospitalId}/Doctors/{DoctorId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> RemoveDoctorFromHospital(string hospitalId, int DoctorId)
        {
            var DoctorHospital = await _unitOfWork.DoctorHospitals
                .FirstOrDefaultAsync(dh => dh.DoctorId == DoctorId && dh.HospitalId == hospitalId && dh.IsActive);

            if (DoctorHospital == null)
            {
                return NotFound($"Doctor with ID {DoctorId} is not assigned to hospital with ID {hospitalId}");
            }

            DoctorHospital.IsActive = false;
            await _unitOfWork.DoctorHospitals.UpdateAsync(DoctorHospital);
            await _unitOfWork.SaveChangesAsync();

            return Ok($"Doctor removed from hospital successfully");
        }

        [HttpGet("Doctors/{DoctorId}/hospitals")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetDoctorHospitals(int DoctorId)
        {
            var Doctor = await _unitOfWork.Doctors.GetDoctorWithHospitalsAsync(DoctorId);
            if (Doctor == null)
            {
                return NotFound($"Doctor with ID {DoctorId} not found");
            }

            var hospitals = Doctor.DoctorHospitals
                .Where(dh => dh.IsActive)
                .Select(dh => new HospitalSimpleDTO
                {
                    HospitalId = dh.Hospital.HospitalId,
                    Name = dh.Hospital.Name,
                    Location = dh.Hospital.Location
                });

            return Ok(new
            {
                Doctor = new
                {
                    Doctor.DoctorId,
                    Doctor.Name,
                    Doctor.Specialty
                },
                HospitalCount = hospitals.Count(),
                Hospitals = hospitals
            });
        }

        // Technician management endpoints
        [HttpPost("{hospitalId}/Technicians")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AddTechnician(string hospitalId, TechnicianDTO TechnicianDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(hospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var Technician = new Technician
            {
                Name = TechnicianDTO.Name,
                Department = TechnicianDTO.Department,
                HospitalId = hospitalId,
                UserId = TechnicianDTO.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Technicians.CreateAsync(Technician);
            await _unitOfWork.SaveChangesAsync();

            return Ok($"Technician '{Technician.Name}' added to hospital '{hospital.Name}' successfully");
        }

        [HttpGet("{hospitalId}/Technicians")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetHospitalTechnicians(string hospitalId)
        {
            var hospital = await _unitOfWork.Hospitals.GetHospitalWithTechniciansAsync(hospitalId);

            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var Technicians = hospital.Technicians.Select(t => new
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
                TechnicianCount = Technicians.Count(),
                Technicians = Technicians
            });
        }
    }
}
