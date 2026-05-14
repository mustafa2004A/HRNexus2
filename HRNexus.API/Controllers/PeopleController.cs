using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Core;
using HRNexus.Business.Models.Files;
using HRNexus.Business.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRNexus.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/people")]
[Authorize(Policy = AuthorizationPolicyNames.HrOrAdmin)]
public sealed class PeopleController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly IPersonContactService _personContactService;
    private readonly IAddressService _addressService;
    private readonly IPersonIdentifierService _personIdentifierService;

    public PeopleController(
        IPersonService personService,
        IPersonContactService personContactService,
        IAddressService addressService,
        IPersonIdentifierService personIdentifierService)
    {
        _personService = personService;
        _personContactService = personContactService;
        _addressService = addressService;
        _personIdentifierService = personIdentifierService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<PersonDto>>> List(
        [FromQuery] string? search,
        [FromQuery] bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var result = await _personService.ListAsync(search, includeDeleted, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{personId:int}")]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonDto>> GetById(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        CancellationToken cancellationToken)
    {
        var result = await _personService.GetByIdAsync(personId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonDto>> Create([FromBody] CreatePersonRequest request, CancellationToken cancellationToken)
    {
        var result = await _personService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { personId = result.PersonId }, result);
    }

    [HttpPut("{personId:int}")]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonDto>> Update(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromBody] UpdatePersonRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personService.UpdateAsync(personId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{personId:int}/photo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PersonPhotoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonPhotoDto>> UploadPhoto(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromForm] UploadPersonPhotoForm request,
        CancellationToken cancellationToken)
    {
        if (request.Photo is null)
        {
            return BadRequest("Uploaded photo is required.");
        }

        using var stream = request.Photo.OpenReadStream();
        var file = new FileUploadContent(
            stream,
            request.Photo.FileName,
            request.Photo.ContentType,
            request.Photo.Length);

        var result = await _personService.UploadPhotoAsync(personId, file, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{personId:int}")]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonDto>> Delete(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        CancellationToken cancellationToken)
    {
        var result = await _personService.DeleteAsync(personId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{personId:int}/contacts")]
    [ProducesResponseType(typeof(IReadOnlyList<PersonContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<PersonContactDto>>> GetContacts(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        CancellationToken cancellationToken)
    {
        var result = await _personContactService.GetByPersonAsync(personId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{personId:int}/contacts/{contactId:int}")]
    [ProducesResponseType(typeof(PersonContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonContactDto>> GetContact(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromRoute, Range(1, int.MaxValue)] int contactId,
        CancellationToken cancellationToken)
    {
        var result = await _personContactService.GetByIdAsync(personId, contactId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{personId:int}/contacts")]
    [ProducesResponseType(typeof(PersonContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonContactDto>> CreateContact(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromBody] CreatePersonContactRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personContactService.CreateAsync(personId, request, cancellationToken);
        return CreatedAtAction(nameof(GetContact), new { personId, contactId = result.ContactId }, result);
    }

    [HttpPut("{personId:int}/contacts/{contactId:int}")]
    [ProducesResponseType(typeof(PersonContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonContactDto>> UpdateContact(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromRoute, Range(1, int.MaxValue)] int contactId,
        [FromBody] UpdatePersonContactRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personContactService.UpdateAsync(personId, contactId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{personId:int}/contacts/{contactId:int}")]
    [ProducesResponseType(typeof(PersonContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonContactDto>> DeleteContact(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromRoute, Range(1, int.MaxValue)] int contactId,
        CancellationToken cancellationToken)
    {
        var result = await _personContactService.DeleteAsync(personId, contactId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{personId:int}/addresses")]
    [ProducesResponseType(typeof(IReadOnlyList<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetAddresses(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        CancellationToken cancellationToken)
    {
        var result = await _addressService.GetByPersonAsync(personId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{personId:int}/addresses/{addressId:int}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddressDto>> GetAddress(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromRoute, Range(1, int.MaxValue)] int addressId,
        CancellationToken cancellationToken)
    {
        var result = await _addressService.GetByIdAsync(personId, addressId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{personId:int}/addresses")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddressDto>> CreateAddress(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromBody] CreateAddressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _addressService.CreateAsync(personId, request, cancellationToken);
        return CreatedAtAction(nameof(GetAddress), new { personId, addressId = result.AddressId }, result);
    }

    [HttpPut("{personId:int}/addresses/{addressId:int}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddressDto>> UpdateAddress(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromRoute, Range(1, int.MaxValue)] int addressId,
        [FromBody] UpdateAddressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _addressService.UpdateAsync(personId, addressId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{personId:int}/addresses/{addressId:int}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddressDto>> DeleteAddress(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromRoute, Range(1, int.MaxValue)] int addressId,
        CancellationToken cancellationToken)
    {
        var result = await _addressService.DeleteAsync(personId, addressId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{personId:int}/identifiers")]
    [ProducesResponseType(typeof(IReadOnlyList<PersonIdentifierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<PersonIdentifierDto>>> GetIdentifiers(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        CancellationToken cancellationToken)
    {
        var result = await _personIdentifierService.GetByPersonAsync(personId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{personId:int}/identifiers/{identifierId:int}")]
    [ProducesResponseType(typeof(PersonIdentifierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonIdentifierDto>> GetIdentifier(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromRoute, Range(1, int.MaxValue)] int identifierId,
        CancellationToken cancellationToken)
    {
        var result = await _personIdentifierService.GetByIdAsync(personId, identifierId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{personId:int}/identifiers")]
    [ProducesResponseType(typeof(PersonIdentifierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonIdentifierDto>> CreateIdentifier(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromBody] CreatePersonIdentifierRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personIdentifierService.CreateAsync(personId, request, cancellationToken);
        return CreatedAtAction(nameof(GetIdentifier), new { personId, identifierId = result.PersonIdentifierId }, result);
    }

    [HttpPut("{personId:int}/identifiers/{identifierId:int}")]
    [ProducesResponseType(typeof(PersonIdentifierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonIdentifierDto>> UpdateIdentifier(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromRoute, Range(1, int.MaxValue)] int identifierId,
        [FromBody] UpdatePersonIdentifierRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _personIdentifierService.UpdateAsync(personId, identifierId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{personId:int}/identifiers/{identifierId:int}")]
    [ProducesResponseType(typeof(PersonIdentifierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonIdentifierDto>> DeleteIdentifier(
        [FromRoute, Range(1, int.MaxValue)] int personId,
        [FromRoute, Range(1, int.MaxValue)] int identifierId,
        CancellationToken cancellationToken)
    {
        var result = await _personIdentifierService.DeleteAsync(personId, identifierId, cancellationToken);
        return Ok(result);
    }
}

public sealed class UploadPersonPhotoForm
{
    [Required]
    [FromForm(Name = "photo")]
    public IFormFile? Photo { get; set; }
}
