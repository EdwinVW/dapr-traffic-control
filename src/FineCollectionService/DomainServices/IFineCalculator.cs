namespace FineCollectionService.DomainServices
{
    public interface IFineCalculator
    {
        public int CalculateFine(int violationInKmh);
    }
}