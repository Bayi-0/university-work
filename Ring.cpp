#include <iostream>
#include <stdexcept>

class Ring {
private:
    // вузлова структура для двозв'язного кільцевого списку
    struct Node {
        int data;
        Node* next;
        Node* prev;
        Node(int val) : data(val), next(nullptr), prev(nullptr) {}
    };

    Node* current;
    size_t count;

    // допоміжний метод очищення
    void Clear() {
        while (count > 0) {
            int temp;
            *this >> temp;
        }
    }

public:
    // конструктор за замовчуванням
    Ring() : current(nullptr), count(0) {}

    // конструктор з масиву
    Ring(const int* arr, size_t size) : current(nullptr), count(0) {
        if (arr == nullptr && size > 0) throw std::invalid_argument("Null array passed");
        for (size_t i = 0; i < size; ++i) {
            *this << arr[i]; 
        }
    }

    // конструктор копіювання
    Ring(const Ring& other) : current(nullptr), count(0) {
        for (size_t i = 0; i < other.count; ++i) {
            *this << other[i];
        }
    }

    // конструктор переміщення
    Ring(Ring&& other) noexcept : current(other.current), count(other.count) {
        other.current = nullptr;
        other.count = 0;
    }

    // деструктор
    ~Ring() {
        Clear();
    }

    size_t getSize() const {
        return count;
    }

    // перевантаженн копіюючого присвоювання
    Ring& operator=(const Ring& other) {
        if (this != &other) {
            Clear();
            for (size_t i = 0; i < other.count; ++i) {
                *this << other[i]; 
            }
        }
        return *this;
    }

    // перевантаження переміщуючого присвоювання
    Ring& operator=(Ring&& other) noexcept {
        if (this != &other) {
            Clear();
            current = other.current;
            count = other.count;
            other.current = nullptr;
            other.count = 0;
        }
        return *this;
    }

    // запис елементу в кільце <<
    Ring& operator<<(int val) {
        Node* newNode = new Node(val);
        if (!current) {
            current = newNode;
            current->next = current;
            current->prev = current;
        } else {
            Node* tail = current->prev;
            tail->next = newNode;
            newNode->prev = tail;
            newNode->next = current;
            current->prev = newNode;
        }
        count++;
        return *this;
    }

    // зчитування з вилученням елементу >>
    Ring& operator>>(int& val) {
        if (!current) throw std::out_of_range("The ring is empty");
        val = current->data;
        
        if (count == 1) {
            delete current;
            current = nullptr;
        } else {
            Node* toDelete = current;
            current->prev->next = current->next;
            current->next->prev = current->prev;
            current = current->next; 
            delete toDelete;
        }
        count--;
        return *this;
    }

    // зчитування без вилучення елементу (“>”)
    Ring& operator>(int& val) {
        if (!current) throw std::out_of_range("The ring is empty");
        val = current->data;
        return *this;
    }

    // переміщення вказівника за годинниковою стрілкою ++
    Ring& operator++() {
        if (current) current = current->next;
        return *this;
    }

    // переміщення вказівника проти годинникової стрілки --
    Ring& operator--() {
        if (current) current = current->prev;
        return *this;
    }

    // доступ за індексом (перевантаження [])
    int& operator[](size_t index) const {
        if (!current) throw std::out_of_range("The ring is empty");
        Node* temp = current;
        index %= count;
        for (size_t i = 0; i < index; ++i) {
            temp = temp->next;
        }
        return temp->data;
    }

    // перетворення об'єкта до одновимірного масиву (операція перетворення типу)
    explicit operator int*() const {
        if (!current) return nullptr;
        int* arr = new int[count];
        for (size_t i = 0; i < count; ++i) {
            arr[i] = (*this)[i]; 
        }
        return arr;
    }


    bool operator==(const Ring& other) const {
        if (count != other.count) return false;
        if (count == 0) return true;
        
        for (size_t i = 0; i < count; ++i) {
            if ((*this)[i] != other[i]) return false;
        }
        return true;
    }

    bool operator!=(const Ring& other) const {
        return !(*this == other);
    }

    bool operator>(const Ring& other) const {
        return count > other.count;
    }

    bool operator<(const Ring& other) const {
        return count < other.count;
    }


    friend std::ostream& operator<<(std::ostream& os, const Ring& r) {
        if (!r.current) {
            os << "[Empty ring]";
            return os;
        }
        os << "[ ";
        for (size_t i = 0; i < r.count; ++i) {
            os << r[i] << " ";
        }
        os << "]";
        return os;
    }

    friend std::istream& operator>>(std::istream& is, Ring& r) {
        int val;
        is >> val;
        r << val;
        return is;
    }
};


int main() {
    try {
        std::cout << "1. Ring creation from array:\n";
        int arr[] = {10, 20, 30, 42};
        Ring r1(arr, 4);
        std::cout << "r1: " << r1 << "\n\n";

        std::cout << "2. Test of specific ring operators (>, >>, <<):\n";
        int readVal;
        r1 > readVal;
        std::cout << "Read without extraction (r1 > val): " << readVal << "\n";
        std::cout << "r1 after reading: " << r1 << "\n";

        int extVal;
        r1 >> extVal;
        std::cout << "Extracted element (r1 >> val): " << extVal << "\n";
        std::cout << "r1 after extraction: " << r1 << "\n";

        r1 << 50;  
        std::cout << "Added element 50 (r1 << 50): " << r1 << "\n\n";

        std::cout << "3. Test of pointer movement (++ and --):\n";
        ++r1;
        std::cout << "After ++: " << r1 << "\n";
        --r1;
        --r1;
        std::cout << "After double --: " << r1 << "\n\n";

        std::cout << "4. Test of index access operator []:\n";
        std::cout << "r1[0] = " << r1[0] << ", r1[2] = " << r1[2] << "\n\n";

        std::cout << "5. Test of type conversion (to array):\n";
        int* dynArr = static_cast<int*>(r1);

        std::cout << "Elements from the converted array: [ ";
        for (size_t i = 0; i < r1.getSize(); ++i) {
            std::cout << dynArr[i] << " ";
        }
        std::cout << "]\n\n";

        delete[] dynArr; 

        std::cout << "6. Test of comparison and assignment:\n";
        Ring r2;
        r2 = r1; 
        std::cout << "r2 created via assignment: " << r2 << "\n";
        std::cout << "Is r1 == r2? " << (r1 == r2 ? "Yes" : "No") << "\n\n";

    } catch (const std::exception& e) {
        std::cerr << "Error: " << e.what() << "\n";
    }

    return 0;
}