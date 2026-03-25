#include <iostream>
#include <cmath>
#include <stdexcept>

class SparseVector {
private:
    // вкладена структура для реалізації динамічного списку
    struct Node {
        size_t index;
        double value;
        Node* next;
        
        Node(size_t idx, double val, Node* nxt = nullptr) 
            : index(idx), value(val), next(nxt) {}
    };

    size_t dimension;
    Node* head;

    // допоміжна функція очищення пам'яті
    void Clear() {
        Node* current = head;
        while (current != nullptr) {
            Node* nextNode = current->next;
            delete current;
            current = nextNode;
        }
        head = nullptr;
    }

    // допоміжна функція для копіювання
    void CopyFrom(const SparseVector& other) {
        dimension = other.dimension;
        if (other.head == nullptr) {
            head = nullptr;
            return;
        }
        head = new Node(other.head->index, other.head->value);
        Node* current = head;
        Node* otherCurrent = other.head->next;
        
        while (otherCurrent != nullptr) {
            current->next = new Node(otherCurrent->index, otherCurrent->value);
            current = current->next;
            otherCurrent = otherCurrent->next;
        }
    }

public:
    // конструктор за замовчуванням
    SparseVector() : dimension(0), head(nullptr) {}

    // конструктор з використанням масиву чисел
    SparseVector(size_t dim, const double* arr, size_t arrSize) : dimension(dim), head(nullptr) {
        if (arrSize > dim) {
            throw std::invalid_argument("Array size mustn't be greater than dimension");
        }
        for (size_t i = 0; i < arrSize; ++i) {
            if (arr[i] != 0.0) {
                setValue(i, arr[i]);
            }
        }
    }

    // конструктор копіювання
    SparseVector(const SparseVector& other) : dimension(0), head(nullptr) {
        CopyFrom(other);
    }

    // переміщуючий конструктор
    SparseVector(SparseVector&& other) noexcept : dimension(other.dimension), head(other.head) {
        other.dimension = 0;
        other.head = nullptr;
    }

    // деструктор
    ~SparseVector() {
        Clear();
    }

    // перевантаження операції присвоєння
    SparseVector& operator=(const SparseVector& other) {
        if (this != &other) {
            Clear();
            CopyFrom(other);
        }
        return *this;
    }

    // перевантаження переміщуючого присвоєння
    SparseVector& operator=(SparseVector&& other) noexcept {
        if (this != &other) {
            Clear();
            dimension = other.dimension;
            head = other.head;
            
            other.dimension = 0;
            other.head = nullptr;
        }
        return *this;
    }

    // функція доступу
    double getValue(size_t index) const {
        if (index >= dimension) {
            throw std::out_of_range("Index is out of vector's dimension");
        }
        Node* current = head;
        while (current != nullptr) {
            if (current->index == index) return current->value;
            if (current->index > index) break; 
            current = current->next;
        }
        return 0.0;
    }

    // функція ініціалізації/зміни значення
    void setValue(size_t index, double value) {
        if (index >= dimension) {
            throw std::out_of_range("Index is out of vector's dimension");
        }

        Node* current = head;
        Node* prev = nullptr;

        while (current != nullptr && current->index < index) {
            prev = current;
            current = current->next;
        }

        if (current != nullptr && current->index == index) {
            if (value == 0.0) { 
                if (prev == nullptr) head = current->next;
                else prev->next = current->next;
                delete current;
            } else { 
                current->value = value;
            }
        } else if (value != 0.0) { 
            Node* newNode = new Node(index, value, current);
            if (prev == nullptr) head = newNode;
            else prev->next = newNode;
        }
    }

    // функція доступу до розмірності
    size_t getDimension() const {
        return dimension;
    }

    // функціональність

    SparseVector Add(const SparseVector& other) const {
        if (dimension != other.dimension) {
            throw std::invalid_argument("Vectors' dimensions must match");
        }
        SparseVector result(*this);
        Node* otherCurrent = other.head;
        while (otherCurrent != nullptr) {
            double currentVal = result.getValue(otherCurrent->index);
            result.setValue(otherCurrent->index, currentVal + otherCurrent->value);
            otherCurrent = otherCurrent->next;
        }
        return result;
    }

    SparseVector Subtract(const SparseVector& other) const {
        if (dimension != other.dimension) {
            throw std::invalid_argument("Vectors' dimensions must match");
        }
        SparseVector result(*this);
        Node* otherCurrent = other.head;
        while (otherCurrent != nullptr) {
            double currentVal = result.getValue(otherCurrent->index);
            result.setValue(otherCurrent->index, currentVal - otherCurrent->value);
            otherCurrent = otherCurrent->next;
        }
        return result;
    }

    SparseVector MultiplyByScalar(double scalar) const {
        SparseVector result(dimension, nullptr, 0); 
        if (scalar == 0.0) return result;

        Node* current = head;
        while (current != nullptr) {
            result.setValue(current->index, current->value * scalar);
            current = current->next;
        }
        return result;
    }

    double CalculateMagnitude() const {
        double sumOfSquares = 0.0;
        Node* current = head;
        while (current != nullptr) {
            sumOfSquares += current->value * current->value;
            current = current->next;
        }
        return std::sqrt(sumOfSquares);
    }

    double CalculateDotProduct(const SparseVector& other) const {
        if (dimension != other.dimension) {
            throw std::invalid_argument("Vectors' dimensions must match");
        }
        double result = 0.0;
        Node* current = head;
        while (current != nullptr) {
            result += current->value * other.getValue(current->index);
            current = current->next;
        }
        return result;
    }

    bool IsEqual(const SparseVector& other) const {
        if (dimension != other.dimension) return false;
        Node* curr1 = head;
        Node* curr2 = other.head;
        while (curr1 != nullptr && curr2 != nullptr) {
            if (curr1->index != curr2->index) return false;
            if (std::abs(curr1->value - curr2->value) > 1e-9) return false;
            curr1 = curr1->next;
            curr2 = curr2->next;
        }
        return curr1 == nullptr && curr2 == nullptr;
    }

    // ввід та вивід

    void Read() {
        Clear();
        std::cout << "Enter the vector's dimension: ";
        if (!(std::cin >> dimension)) {
            throw std::runtime_error("Error");
        }
        size_t elementsCount;
        std::cout << "Enter the number of non-zero elements: ";
        if (!(std::cin >> elementsCount)) {
             throw std::runtime_error("Error");
        }
        if (elementsCount > dimension) {
            throw std::invalid_argument("Number of elements can't be greater than dimension");
        }
        for (size_t i = 0; i < elementsCount; ++i) {
            size_t idx;
            double val;
            std::cout << "Enter index (0 - " << dimension - 1 << ") and value: ";
            if (!(std::cin >> idx >> val)) {
                throw std::runtime_error("Error");
            }
            setValue(idx, val); 
        }
    }

    void Print() const {
        std::cout << "[ ";
        Node* current = head;
        for (size_t i = 0; i < dimension; ++i) {
            if (current != nullptr && current->index == i) {
                std::cout << current->value << " ";
                current = current->next;
            } else {
                std::cout << "0 ";
            }
        }
        std::cout << "] (Dimension: " << dimension << ")\n";
    }

    void PrintSparseFormat() const {
        std::cout << "Dimension: " << dimension << " | Non-zero elements: { ";
        Node* current = head;
        while (current != nullptr) {
            std::cout << "[" << current->index << "]:" << current->value << " ";
            current = current->next;
        }
        std::cout << "}\n";
    }
};

int main() {
    try {
        std::cout << "Demonstration of the class\n\n";

        double arr1[] = {1.5, 0.0, 3.0, 0.0, -2.0};
        SparseVector v1(5, arr1, 5);

        double arr2[] = {0.0, 2.5, 3.0, 1.0, 0.0};
        SparseVector v2(5, arr2, 5);

        std::cout << "Vector v1: "; v1.Print();
        std::cout << "Vector v2: "; v2.Print();

        SparseVector vSum = v1.Add(v2);
        std::cout << "\nAddition (v1 + v2): "; vSum.Print();

        SparseVector vDiff = v1.Subtract(v2);
        std::cout << "Subtraction (v1 - v2): "; vDiff.Print();

        SparseVector vMult = v1.MultiplyByScalar(2.0);
        std::cout << "Scalar multiplication (v1 * 2.0): "; vMult.Print();

        std::cout << "\nVector's magnitude v1: " << v1.CalculateMagnitude() << "\n";
        std::cout << "Scalar product of v1 and v2: " << v1.CalculateDotProduct(v2) << "\n";

        std::cout << "Does v1 equal v2? " << (v1.IsEqual(v2) ? "Yes" : "No") << "\n";

    } catch (const std::exception& e) {
        std::cerr << "\nCritical error: " << e.what() << "\n";
    }

    return 0;
}