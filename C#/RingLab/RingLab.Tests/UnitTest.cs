using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RingLab;

namespace RingLab.Tests
{
    [TestClass]
    public class UnitTest
    {
        // 1. створення порожнього кільця
        [TestMethod]
        public void Constructors_EmptyInputs_CreateEmptyRings()
        {
            // arrange
            int[] arr = new int[0];

            // act
            Ring defaultRing = new Ring();
            Ring arrayRing = new Ring(arr);

            // assert
            AssertRing(defaultRing, "[]", 0);
            AssertRing(arrayRing, "[]", 0);
        }

        // 2. створення кільця з масиву
        [TestMethod]
        public void ArrayConstructor_NonemptyArrays_PreservesOrderAndSize()
        {
            // arrange
            int[] single = { 42 };
            int[] several = { 10, 20, 30 };

            // act
            Ring singleRing = new Ring(single);
            Ring severalRing = new Ring(several);

            // assert
            AssertRing(singleRing, "[42]", 1);
            Assert.AreEqual(42, singleRing.Read());
            AssertRing(severalRing, "[10, 20, 30]", 3);
            Assert.AreEqual(10, severalRing.Read());
        }

        // 3. виняток для масиву null
        [TestMethod]
        public void ArrayConstructor_NullArray_ThrowsArgumentNullException()
        {
            // arrange
            int[] arr = null;

            // act
            Action act = () => { new Ring(arr); };

            // assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        // 4. копіювання порожнього кільця
        [TestMethod]
        public void CopyConstructor_EmptyRing_CreatesEmptyCopy()
        {
            // arrange
            Ring original = new Ring();

            // act
            Ring copy = new Ring(original);

            // assert
            AssertRing(original, "[]", 0);
            AssertRing(copy, "[]", 0);
            Assert.AreNotSame(original, copy);
        }

        // 5. копія зберігає поточний елемент і не змінює оригінал
        [TestMethod]
        public void CopyConstructor_ShiftedRing_PreservesPositionAndIndependence()
        {
            // arrange
            Ring original = new Ring(new int[] { 10, 20, 30 });
            original.MoveClockwise();

            // act
            Ring copy = new Ring(original);
            string initialCopy = copy.ToString();
            int initialSize = copy.Size;
            int removed = copy.ReadAndRemove();
            copy.Write(40);
            copy.MoveCounterclockwise();

            // assert
            Assert.AreEqual("[20, 30, 10]", initialCopy);
            Assert.AreEqual(3, initialSize);
            Assert.AreEqual(20, removed);
            AssertRing(copy, "[40, 30, 10]", 3);
            AssertRing(original, "[20, 30, 10]", 3);
            Assert.AreEqual(20, original.Read());
        }

        // 6. виняток для копіювання null
        [TestMethod]
        public void CopyConstructor_NullSource_ThrowsArgumentNullException()
        {
            // arrange
            Ring original = null;

            // act
            Action act = () => { new Ring(original); };

            // assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        // 7. читання не змінює кільце
        [TestMethod]
        public void Read_NonemptyRings_ReturnsCurrentWithoutChangingState()
        {
            // arrange
            Ring single = new Ring(new int[] { 42 });
            Ring several = new Ring(new int[] { 10, 20, 30 });

            // act
            int singleFirst = single.Read();
            int singleSecond = single.Read();
            int severalFirst = several.Read();
            int severalSecond = several.Read();

            // assert
            Assert.AreEqual(42, singleFirst);
            Assert.AreEqual(42, singleSecond);
            Assert.AreEqual(10, severalFirst);
            Assert.AreEqual(10, severalSecond);
            AssertRing(single, "[42]", 1);
            AssertRing(several, "[10, 20, 30]", 3);
        }

        // 8. виняток для читання порожнього кільця
        [TestMethod]
        public void Read_EmptyRing_ThrowsInvalidOperationException()
        {
            // arrange
            Ring ring = new Ring();

            // act
            Action act = () => ring.Read();

            // assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
            AssertRing(ring, "[]", 0);
        }

        // 9. видалення і рух між вузлами, що залишились
        [TestMethod]
        public void ReadAndRemove_SeveralNodes_ReturnsValueAndPreservesLinks()
        {
            // arrange
            Ring ring = new Ring(new int[] { 10, 20, 30 });

            // act
            int removed = ring.ReadAndRemove();
            int afterRemoval = ring.Read();
            ring.MoveCounterclockwise();
            int afterBackward = ring.Read();
            ring.MoveClockwise();
            int afterForward = ring.Read();

            // assert
            Assert.AreEqual(10, removed);
            Assert.AreEqual(20, afterRemoval);
            Assert.AreEqual(30, afterBackward);
            Assert.AreEqual(20, afterForward);
            AssertRing(ring, "[20, 30]", 2);
        }

        // 10. видалення останнього елемента
        [TestMethod]
        public void ReadAndRemove_SingleNode_LeavesEmptyRing()
        {
            // arrange
            Ring ring = new Ring(new int[] { 42 });

            // act
            int removed = ring.ReadAndRemove();

            // assert
            Assert.AreEqual(42, removed);
            AssertRing(ring, "[]", 0);
            Assert.ThrowsExactly<InvalidOperationException>(() => ring.Read());
        }

        // 11. виняток для видалення з порожнього кільця
        [TestMethod]
        public void ReadAndRemove_EmptyRing_ThrowsInvalidOperationException()
        {
            // arrange
            Ring ring = new Ring();

            // act
            Action act = () => ring.ReadAndRemove();

            // assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
            AssertRing(ring, "[]", 0);
        }

        // 12. запис першого елемента і рух в обох напрямках
        [TestMethod]
        public void Write_EmptyRing_CreatesCurrentAndCircularLinks()
        {
            // arrange
            Ring ring = new Ring();

            // act
            ring.Write(10);
            int afterWrite = ring.Read();
            ring.MoveClockwise();
            int afterForward = ring.Read();
            ring.MoveCounterclockwise();
            int afterBackward = ring.Read();

            // assert
            Assert.AreEqual(10, afterWrite);
            Assert.AreEqual(10, afterForward);
            Assert.AreEqual(10, afterBackward);
            AssertRing(ring, "[10]", 1);
        }

        // 13. запис перед поточним елементом
        [TestMethod]
        public void Write_NonemptyRing_InsertsBeforeCurrentAndPreservesPosition()
        {
            // arrange
            Ring ring = new Ring(new int[] { 10 });

            // act
            ring.Write(20);
            int currentAfterFirstWrite = ring.Read();
            ring.Write(30);
            int currentAfterSecondWrite = ring.Read();
            string afterWrites = ring.ToString();
            ring.MoveCounterclockwise();
            int previous = ring.Read();

            // assert
            Assert.AreEqual(10, currentAfterFirstWrite);
            Assert.AreEqual(10, currentAfterSecondWrite);
            Assert.AreEqual("[10, 20, 30]", afterWrites);
            Assert.AreEqual(30, previous);
            AssertRing(ring, "[30, 10, 20]", 3);
        }

        // 14. запис нуля, від’ємних чисел і повторів
        [TestMethod]
        public void Write_ZeroNegativeAndDuplicateValues_PreservesAllValues()
        {
            // arrange
            Ring ring = new Ring();

            // act
            ring.Write(0);
            ring.Write(-5);
            ring.Write(0);

            // assert
            AssertRing(ring, "[0, -5, 0]", 3);
        }

        // 15. запис після видалення останнього елемента
        [TestMethod]
        public void Write_AfterRemovingLastNode_ReusesRing()
        {
            // arrange
            Ring ring = new Ring(new int[] { 10 });
            ring.ReadAndRemove();

            // act
            ring.Write(20);

            // assert
            Assert.AreEqual(20, ring.Read());
            AssertRing(ring, "[20]", 1);
        }

        // 16. повний оберт за годинниковою стрілкою
        [TestMethod]
        public void MoveClockwise_FullRevolution_VisitsNodesAndWrapsAround()
        {
            // arrange
            Ring ring = new Ring(new int[] { 10, 20, 30 });

            // act
            ring.MoveClockwise();
            int first = ring.Read();
            ring.MoveClockwise();
            int second = ring.Read();
            ring.MoveClockwise();
            int third = ring.Read();

            // assert
            Assert.AreEqual(20, first);
            Assert.AreEqual(30, second);
            Assert.AreEqual(10, third);
            AssertRing(ring, "[10, 20, 30]", 3);
        }

        // 17. повний оберт проти годинникової стрілки
        [TestMethod]
        public void MoveCounterclockwise_FullRevolution_VisitsNodesAndWrapsAround()
        {
            // arrange
            Ring ring = new Ring(new int[] { 10, 20, 30 });

            // act
            ring.MoveCounterclockwise();
            int first = ring.Read();
            ring.MoveCounterclockwise();
            int second = ring.Read();
            ring.MoveCounterclockwise();
            int third = ring.Read();

            // assert
            Assert.AreEqual(30, first);
            Assert.AreEqual(20, second);
            Assert.AreEqual(10, third);
            AssertRing(ring, "[10, 20, 30]", 3);
        }

        // 18. рух у порожньому кільці та кільці з одним елементом
        [TestMethod]
        public void Movement_EmptyAndSingleNodeRings_LeavesStateUnchanged()
        {
            // arrange
            Ring empty = new Ring();
            Ring single = new Ring(new int[] { 42 });

            // act
            empty.MoveClockwise();
            empty.MoveCounterclockwise();
            single.MoveClockwise();
            int afterForward = single.Read();
            single.MoveCounterclockwise();
            int afterBackward = single.Read();

            // assert
            AssertRing(empty, "[]", 0);
            AssertRing(single, "[42]", 1);
            Assert.AreEqual(42, afterForward);
            Assert.AreEqual(42, afterBackward);
        }

        // 19. сильна рівність однакових кілець
        [TestMethod]
        public void StrongEquals_IdenticalSequencesAndSelf_ReturnsTrue()
        {
            // arrange
            Ring first = new Ring(new int[] { 10, 20, 30 });
            Ring second = new Ring(new int[] { 10, 20, 30 });

            // act
            bool selfEqual = first.StrongEquals(first);
            bool copiesEqual = first.StrongEquals(second);
            bool reverseEqual = second.StrongEquals(first);

            // assert
            Assert.IsTrue(selfEqual);
            Assert.IsTrue(copiesEqual);
            Assert.IsTrue(reverseEqual);
            AssertRing(first, "[10, 20, 30]", 3);
            AssertRing(second, "[10, 20, 30]", 3);
        }

        // 20. сильна нерівність за різних положень, значень чи розмірів
        [TestMethod]
        public void StrongEquals_DifferentPositionValuesOrSize_ReturnsFalse()
        {
            // arrange
            Ring ring = new Ring(new int[] { 10, 20, 30 });
            Ring shifted = new Ring(new int[] { 10, 20, 30 });
            shifted.MoveClockwise();
            Ring different = new Ring(new int[] { 10, 99, 30 });
            Ring shorter = new Ring(new int[] { 10, 20 });

            // act
            bool shiftedEqual = ring.StrongEquals(shifted);
            bool valuesEqual = ring.StrongEquals(different);
            bool sizesEqual = ring.StrongEquals(shorter);

            // assert
            Assert.IsFalse(shiftedEqual);
            Assert.IsFalse(valuesEqual);
            Assert.IsFalse(sizesEqual);
            AssertRing(ring, "[10, 20, 30]", 3);
            AssertRing(shifted, "[20, 30, 10]", 3);
            AssertRing(different, "[10, 99, 30]", 3);
            AssertRing(shorter, "[10, 20]", 2);
        }

        // 21. порівняння порожніх кілець між собою та з непорожнім
        [TestMethod]
        public void Comparisons_EmptyRings_ReturnExpectedEquality()
        {
            // arrange
            Ring first = new Ring();
            Ring second = new Ring();
            Ring nonempty = new Ring(new int[] { 10 });

            // act
            bool strongEmpty = first.StrongEquals(second);
            bool weakEmpty = first.WeakEquals(second);
            bool strongForward = first.StrongEquals(nonempty);
            bool strongBackward = nonempty.StrongEquals(first);
            bool weakForward = first.WeakEquals(nonempty);
            bool weakBackward = nonempty.WeakEquals(first);

            // assert
            Assert.IsTrue(strongEmpty);
            Assert.IsTrue(weakEmpty);
            Assert.IsFalse(strongForward);
            Assert.IsFalse(strongBackward);
            Assert.IsFalse(weakForward);
            Assert.IsFalse(weakBackward);
            AssertRing(first, "[]", 0);
            AssertRing(second, "[]", 0);
            AssertRing(nonempty, "[10]", 1);
        }

        // 22. порівняння з null
        [TestMethod]
        public void Comparisons_NullOther_ReturnFalse()
        {
            // arrange
            Ring empty = new Ring();
            Ring nonempty = new Ring(new int[] { 10 });

            // act
            bool strongEmpty = empty.StrongEquals(null);
            bool weakEmpty = empty.WeakEquals(null);
            bool strongNonempty = nonempty.StrongEquals(null);
            bool weakNonempty = nonempty.WeakEquals(null);

            // assert
            Assert.IsFalse(strongEmpty);
            Assert.IsFalse(weakEmpty);
            Assert.IsFalse(strongNonempty);
            Assert.IsFalse(weakNonempty);
        }

        // 23. слабка рівність однакових і зсунутих кілець
        [TestMethod]
        public void WeakEquals_IdenticalRotatedAndSingleNodeRings_ReturnsTrue()
        {
            // arrange
            Ring first = new Ring(new int[] { 10, 20, 30 });
            Ring identical = new Ring(new int[] { 10, 20, 30 });
            Ring shifted = new Ring(new int[] { 10, 20, 30 });
            shifted.MoveClockwise();
            Ring single = new Ring(new int[] { 42 });
            Ring otherSingle = new Ring(new int[] { 42 });

            // act
            bool same = first.WeakEquals(identical);
            bool rotated = first.WeakEquals(shifted);
            bool reverseRotated = shifted.WeakEquals(first);
            bool oneNode = single.WeakEquals(otherSingle);
            bool selfEqual = first.WeakEquals(first);

            // assert
            Assert.IsTrue(same);
            Assert.IsTrue(rotated);
            Assert.IsTrue(reverseRotated);
            Assert.IsTrue(oneNode);
            Assert.IsTrue(selfEqual);
            AssertRing(first, "[10, 20, 30]", 3);
            AssertRing(identical, "[10, 20, 30]", 3);
            AssertRing(shifted, "[20, 30, 10]", 3);
            AssertRing(single, "[42]", 1);
            AssertRing(otherSingle, "[42]", 1);
        }

        // 24. слабка рівність із повторюваними значеннями
        [TestMethod]
        public void WeakEquals_DuplicateValues_TriesLaterMatchingPosition()
        {
            // arrange
            Ring first = new Ring(new int[] { 1, 2, 1, 3 });
            Ring second = new Ring(new int[] { 1, 3, 1, 2 });

            // act
            bool equal = first.WeakEquals(second);
            bool reverseEqual = second.WeakEquals(first);

            // assert
            Assert.IsTrue(equal);
            Assert.IsTrue(reverseEqual);
            AssertRing(first, "[1, 2, 1, 3]", 4);
            AssertRing(second, "[1, 3, 1, 2]", 4);
        }

        // 25. слабка нерівність за різного порядку, значень чи розмірів
        [TestMethod]
        public void WeakEquals_NonRotationsDifferentValuesOrSize_ReturnsFalse()
        {
            // arrange
            Ring three = new Ring(new int[] { 10, 20, 30 });
            Ring reversed = new Ring(new int[] { 10, 30, 20 });
            Ring four = new Ring(new int[] { 1, 2, 3, 4 });
            Ring permuted = new Ring(new int[] { 1, 3, 2, 4 });
            Ring two = new Ring(new int[] { 10, 20 });
            Ring different = new Ring(new int[] { 10, 99 });
            Ring single = new Ring(new int[] { 10 });
            Ring repeated = new Ring(new int[] { 10, 10 });

            // act
            bool reversedEqual = three.WeakEquals(reversed);
            bool permutedEqual = four.WeakEquals(permuted);
            bool valuesEqual = two.WeakEquals(different);
            bool sizesEqual = single.WeakEquals(repeated);

            // assert
            Assert.IsFalse(reversedEqual);
            Assert.IsFalse(permutedEqual);
            Assert.IsFalse(valuesEqual);
            Assert.IsFalse(sizesEqual);
            AssertRing(three, "[10, 20, 30]", 3);
            AssertRing(reversed, "[10, 30, 20]", 3);
            AssertRing(four, "[1, 2, 3, 4]", 4);
            AssertRing(permuted, "[1, 3, 2, 4]", 4);
            AssertRing(two, "[10, 20]", 2);
            AssertRing(different, "[10, 99]", 2);
            AssertRing(single, "[10]", 1);
            AssertRing(repeated, "[10, 10]", 2);
        }

        // 26. перевірка equals для кільця, null та іншого типу
        [TestMethod]
        public void Equals_ObjectsOfDifferentKinds_UsesStrongEquality()
        {
            // arrange
            Ring ring = new Ring(new int[] { 10, 20, 30 });
            object identical = new Ring(new int[] { 10, 20, 30 });
            object shifted = new Ring(new int[] { 20, 30, 10 });
            object otherType = "[10, 20, 30]";

            // act
            bool same = ring.Equals(identical);
            bool rotated = ring.Equals(shifted);
            bool nullEqual = ring.Equals(null);
            bool stringEqual = ring.Equals(otherType);

            // assert
            Assert.IsTrue(same);
            Assert.IsFalse(rotated);
            Assert.IsFalse(nullEqual);
            Assert.IsFalse(stringEqual);
        }

        // 27. однакові хеш-коди для рівних кілець
        [TestMethod]
        public void GetHashCode_StronglyEqualRings_ReturnsEqualHashes()
        {
            // arrange
            Ring first = new Ring(new int[] { 10, 20, 30 });
            Ring second = new Ring(new int[] { 10, 20, 30 });
            Ring emptyFirst = new Ring();
            Ring emptySecond = new Ring();

            // act
            int firstHash = first.GetHashCode();
            int secondHash = second.GetHashCode();
            int emptyFirstHash = emptyFirst.GetHashCode();
            int emptySecondHash = emptySecond.GetHashCode();
            int repeatedHash = first.GetHashCode();

            // assert
            Assert.AreEqual(firstHash, secondHash);
            Assert.AreEqual(emptyFirstHash, emptySecondHash);
            Assert.AreEqual(firstHash, repeatedHash);
        }

        // 28. рядок для порожнього кільця, одного елемента і зсунутого кільця
        [TestMethod]
        public void ToString_EmptySingleAndShiftedRings_ReturnsExpectedText()
        {
            // arrange
            Ring empty = new Ring();
            Ring single = new Ring(new int[] { 42 });
            Ring shifted = new Ring(new int[] { 10, 20, 30 });
            shifted.MoveClockwise();

            // act
            string emptyText = empty.ToString();
            string singleText = single.ToString();
            string shiftedText = shifted.ToString();

            // assert
            Assert.AreEqual("[]", emptyText);
            Assert.AreEqual("[42]", singleText);
            Assert.AreEqual("[20, 30, 10]", shiftedText);
            Assert.AreEqual(0, empty.Size);
            Assert.AreEqual(1, single.Size);
            Assert.AreEqual(3, shifted.Size);
            Assert.AreEqual(42, single.Read());
            Assert.AreEqual(20, shifted.Read());
            Assert.AreEqual(shiftedText, shifted.ToString());
        }

        // перевірка вмісту і розміру кільця
        private static void AssertRing(Ring ring, string expectedText, int expectedSize)
        {
            Assert.AreEqual(expectedText, ring.ToString());
            Assert.AreEqual(expectedSize, ring.Size);
        }
    }
}