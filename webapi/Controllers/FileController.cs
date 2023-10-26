using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Text;
using webapi.Data;
using webapi.Models;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        //in memory db
        private readonly webapiContext _context;

        public FileController(webapiContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Process([FromForm] int caseId, IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null)
            {
                return BadRequest("Process Error: No file submitted");
            }

            // We do some internal application validation here with our caseId

            try
            {
                // get a guid to use as the filename as they're highly unique
                var guid = Guid.NewGuid().ToString();
                var newimage = string.Format("{0}.{1}", guid, file.FileName.Split('.').LastOrDefault());
                // save to local path
                var path = "File/";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path += newimage;
                using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream, cancellationToken);

                Attachment attachment = new Attachment
                {
                    Filename = Path.GetFileNameWithoutExtension(file.FileName),
                    Filetype = Path.GetExtension(file.FileName).Replace(".", String.Empty),
                    FileSize = file.Length,
                    CreatedOn = DateTime.Now,
                    CaseId = caseId,
                    Guid = guid
                };

                _context.Add(attachment);
                await _context.SaveChangesAsync();

                return Ok(guid);
            }
            catch (Exception e)
            {
                return BadRequest($"Process Error: {e.Message}"); // Oops!
            }
        }

        // DELETE: api/RaffleImagesUpload/
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpDelete]
        public async Task<ActionResult> Revert()
        {

            // The server id will be send in the delete request body as plain text
            using StreamReader reader = new(Request.Body, Encoding.UTF8);
            string guid = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(guid))
            {
                return BadRequest("Revert Error: Invalid unique file ID");
            }
            var attachment = _context.Attachment.FirstOrDefault(i => i.Guid == guid);
            // We do some internal application validation here
            try
            {

                if (attachment is null)
                {
                    return NotFound("Revert Error: File not found");
                }
                var imageKey = string.Format("{0}.{1}", attachment.Guid, attachment.Filetype);
                var path = "File/" + imageKey;
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                attachment.Deleted = true;
                _context.Update(attachment);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(string.Format("Revert Error:'{0}' when writing an object", e.Message));
            }
        }

        [HttpGet("Load/{id}")]
        public async Task<IActionResult> Load(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Load Error: Invalid parameters");
            }
            var attachment = await _context.Attachment.SingleOrDefaultAsync(i => i.Guid.Equals(id));
            if (attachment is null)
            {
                return NotFound("Load Error: File not found");
            }

            var imageKey = string.Format("{0}.{1}", attachment.Guid, attachment.Filetype);
            var url = "File/" + imageKey;

            using Stream ImageStream = await new HttpClient().GetStreamAsync(url);
            Response.Headers.Append("Content-Disposition", new ContentDisposition
            {
                FileName = string.Format("{0}.{1}", attachment.Filename, attachment.Filetype),
                Inline = true // false = prompt the user for downloading; true = browser to try to show the file inline
            }.ToString());
            return File(ImageStream, "image/" + attachment.Filetype);
        }
    }
}
