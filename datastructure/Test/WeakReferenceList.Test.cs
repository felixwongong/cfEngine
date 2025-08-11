using System;
using System.Linq;
using NUnit.Framework;

namespace cfEngine.DataStructure.Test;

[TestFixture]
public class WeakReferenceListTests
{
    private static void ForceFullGC()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private sealed class Dummy
    {
        public readonly int Id;
        public Dummy(int id) => Id = id;
        public override string ToString() => $"Dummy({Id})";
    }

    [Test]
    public void Add_IncreasesCount_AndStoresWeakReferences()
    {
        var list = new WeakReferenceList<Dummy>();
        var a = new Dummy(1);
        var b = new Dummy(2);

        list.Add(a);
        list.Add(b);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.All(wr => wr.TryGetTarget(out _)), Is.True);

        Assert.That(list[0].TryGetTarget(out var t0) && ReferenceEquals(t0, a), Is.True);
        Assert.That(list[1].TryGetTarget(out var t1) && ReferenceEquals(t1, b), Is.True);
    }

    [Test]
    public void Remove_RemovesOnlyTheMatchingInstance_AndKeepsOthers()
    {
        var list = new WeakReferenceList<Dummy>();
        var a1 = new Dummy(1);
        var a2 = new Dummy(1); // different instance, same content
        var b = new Dummy(2);

        list.Add(a1);
        list.Add(a2);
        list.Add(b);

        list.Remove(a1);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.All(wr => wr.TryGetTarget(out _)), Is.True);

        Assert.That(list.Any(wr => wr.TryGetTarget(out var t) && ReferenceEquals(t, a2)), Is.True);
        Assert.That(list.Any(wr => wr.TryGetTarget(out var t) && ReferenceEquals(t, b)), Is.True);
        Assert.That(list.Any(wr => wr.TryGetTarget(out var t) && ReferenceEquals(t, a1)), Is.False);
    }

    [Test]
    public void Remove_WithNullItem_SweepsDeadReferences()
    {
        var list = new WeakReferenceList<Dummy>();
        WeakReference<Dummy> deadProbe;

        var live = new Dummy(42);
        list.Add(live);

        // Add a soon-to-be-dead object
        {
            var temp = new Dummy(99);
            list.Add(temp);
            deadProbe = new WeakReference<Dummy>(temp);
        }

        ForceFullGC();
        Assert.That(deadProbe.TryGetTarget(out _), Is.False, "Temp should be collected");

        // Sweep dead entries by calling Remove(null)
        list.Remove(item: null);

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list[0].TryGetTarget(out var survivor), Is.True);
        Assert.That(ReferenceEquals(survivor, live), Is.True);
    }

    [Test]
    public void Remove_OnEmptyList_DoesNotThrow()
    {
        var list = new WeakReferenceList<Dummy>();
        Assert.DoesNotThrow(() => list.Remove(item: null));
        Assert.That(list, Is.Empty);
    }

    [Test]
    public void Remove_IsIdempotent_WhenItemNotPresent()
    {
        var list = new WeakReferenceList<Dummy>();
        var a = new Dummy(1);
        var b = new Dummy(2);

        list.Add(a);
        list.Add(b);

        list.Remove(new Dummy(999));
        list.Remove(new Dummy(999));

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.All(wr => wr.TryGetTarget(out _)), Is.True);
    }

    [Test]
    public void Add_NullItem_CreatesNullTarget_WhichIsRemovedOnCleanup()
    {
        var list = new WeakReferenceList<Dummy>();

        list.Add(item: null);
        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list[0].TryGetTarget(out _), Is.False);

        list.Remove(item: null); // sweep
        Assert.That(list, Is.Empty);
    }

    [Test]
    public void AfterRemove_NoDeadOrRemovedTargetsRemain()
    {
        var list = new WeakReferenceList<Dummy>();
        var keep = new Dummy(10);
        var removed = new Dummy(20);

        list.Add(keep);
        list.Add(removed);

        // Add an entry that becomes dead
        {
            var temp = new Dummy(30);
            list.Add(temp);
        }

        ForceFullGC();

        list.Remove(removed);

        // All remaining entries must be alive and not the removed instance
        Assert.That(list.All(wr => wr.TryGetTarget(out var t) && !ReferenceEquals(t, removed)), Is.True);

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list[0].TryGetTarget(out var survivor), Is.True);
        Assert.That(ReferenceEquals(survivor, keep), Is.True);
    }
}