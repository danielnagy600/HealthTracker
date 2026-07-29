namespace HealthTracker.SharedKernel.Abstractions;

/// <summary>
/// Az idő absztrakciója. Azért nem használunk közvetlenül DateTimeOffset.Now-t
/// az üzleti logikában, mert akkor az időfüggő kódot lehetetlen lenne tesztelni.
/// A tesztek egy rögzített idejű "hamis órát" adnak be helyette.
///
/// Ez a SharedKernelben van, mert több modul is használhatja (közös, keresztmetsző igény).
/// </summary>
public interface IClock
{
    DateTimeOffset Now { get; }
}
