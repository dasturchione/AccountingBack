namespace Application.Features.FaMovements;

public class FaMovementUpdateDto : FaMovementBaseDto
{
    public short StateId { get; set; } = SharedKernel.Constants.StateIdConst.ACTIVE;
}
