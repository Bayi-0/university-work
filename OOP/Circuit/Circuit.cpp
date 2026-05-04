#include <iostream>

using namespace std;

// базовий абстрактний клас
class CircuitElement {
protected:
    int nodeA;
    int nodeB;

public:
    CircuitElement() : nodeA(0), nodeB(0) {}
    CircuitElement(int nA, int nB) : nodeA(nA), nodeB(nB) {}
    CircuitElement(const CircuitElement& other) : nodeA(other.nodeA), nodeB(other.nodeB) {}
    
    virtual ~CircuitElement() {}

    virtual void PrintInfo() const = 0;

    int getNodeA() const { return nodeA; }
    int getNodeB() const { return nodeB; }
};

// похідний клас 1: Резистор
class Resistor : public CircuitElement {
private:
    double resistance;

public:
    Resistor() : CircuitElement(), resistance(0.0) {}
    Resistor(int nA, int nB, double r) : CircuitElement(nA, nB), resistance(r) {}
    Resistor(const Resistor& other) : CircuitElement(other), resistance(other.resistance) {}

    void PrintInfo() const override {
        cout << "Resistor [Nodes: " << nodeA << "-" << nodeB 
             << "] | Resistance: " << resistance << " O" << endl;
    }
};

// похідний клас 2: Конденсатор
class Capacitor : public CircuitElement {
private:
    double capacitance;

public:
    Capacitor() : CircuitElement(), capacitance(0.0) {}
    Capacitor(int nA, int nB, double c) : CircuitElement(nA, nB), capacitance(c) {}
    Capacitor(const Capacitor& other) : CircuitElement(other), capacitance(other.capacitance) {}

    void PrintInfo() const override {
        cout << "Capacitor [Nodes: " << nodeA << "-" << nodeB 
             << "] | Capacitance: " << capacitance << " F" << endl;
    }
};

//похідний клас 3: Котушка індуктивності
class Inductor : public CircuitElement {
private:
    double inductance;

public:
    Inductor() : CircuitElement(), inductance(0.0) {}
    Inductor(int nA, int nB, double l) : CircuitElement(nA, nB), inductance(l) {}
    Inductor(const Inductor& other) : CircuitElement(other), inductance(other.inductance) {}

    void PrintInfo() const override {
        cout << "Inductor [Nodes: " << nodeA << "-" << nodeB 
             << "] | Inductance: " << inductance << " H" << endl;
    }
};

// клас-контейнер 
class Circuit {
private:
    CircuitElement** elements; 
    int size;
    int capacity;

    // допоміжна функція для збільшення місткості масиву
    void resize() {
        capacity = (capacity == 0) ? 2 : capacity * 2;
        CircuitElement** newElements = new CircuitElement*[capacity];
        
        for (int i = 0; i < size; ++i) {
            newElements[i] = elements[i];
        }
        
        delete[] elements; 
        elements = newElements;
    }

    // внутрішня рекурсивна функція для пошуку в глибину
    void dfs(int currentNode, bool* visited) const {
        visited[currentNode] = true;
        
        // шукаємо сусідні вузли, перебираючи всі елементи ланцюга
        for (int i = 0; i < size; ++i) {
            int nA = elements[i]->getNodeA();
            int nB = elements[i]->getNodeB();
            
            if (nA == currentNode && !visited[nB]) {
                dfs(nB, visited);
            } else if (nB == currentNode && !visited[nA]) {
                dfs(nA, visited);
            }
        }
    }

public:
    // конструктор за замовчуванням
    Circuit() : elements(nullptr), size(0), capacity(0) {}

    // деструктор
    ~Circuit() {
        for (int i = 0; i < size; ++i) {
            delete elements[i];
        }
        delete[] elements;
    }

    // додавання нового елемента
    void AddElement(CircuitElement* element) {
        if (element != nullptr) {
            if (size == capacity) {
                resize();
            }
            elements[size++] = element;
        }
    }

    // поліморфний вивід списку об'єктів
    void PrintAllElements() const {
        for (int i = 0; i < size; ++i) {
            elements[i]->PrintInfo();
        }
    }

    // перевірка на зв'язність
    bool CheckConnectivity() const {
        if (size == 0) return true;

        int maxNode = 0;
        for (int i = 0; i < size; ++i) {
            if (elements[i]->getNodeA() > maxNode) maxNode = elements[i]->getNodeA();
            if (elements[i]->getNodeB() > maxNode) maxNode = elements[i]->getNodeB();
        }

        bool* exists = new bool[maxNode + 1];
        bool* visited = new bool[maxNode + 1];
        
        for (int i = 0; i <= maxNode; ++i) {
            exists[i] = false;
            visited[i] = false;
        }

        for (int i = 0; i < size; ++i) {
            exists[elements[i]->getNodeA()] = true;
            exists[elements[i]->getNodeB()] = true;
        }

        int startNode = -1;
        for (int i = 0; i <= maxNode; ++i) {
            if (exists[i]) {
                startNode = i;
                break;
            }
        }

        if (startNode != -1) {
            dfs(startNode, visited);
        }

        bool isConnected = true;
        for (int i = 0; i <= maxNode; ++i) {
            if (exists[i] && !visited[i]) {
                isConnected = false;
                break;
            }
        }

        delete[] exists;
        delete[] visited;

        return isConnected;
    }
};


int main() {
    // демонстрація конструкторів та базового поліморфізму
    cout << "- Constructors and polymorphism demonstration -" << endl;

    // конструктор за замовчуванням
    Resistor defaultResistor;
    cout << "Default constructor (Resistor):" << endl;
    cout << "-> "; 
    defaultResistor.PrintInfo();

    // ініціалізуючий конструктор
    Capacitor initCapacitor(10, 11, 0.022);
    cout << "Initializing constructor (Capacitor):" << endl;
    cout << "-> ";
    initCapacitor.PrintInfo();

    // конструктор копіювання
    Capacitor copiedCapacitor(initCapacitor);
    cout << "Copy constructor (Copied Capacitor):" << endl;
    cout << "-> ";
    copiedCapacitor.PrintInfo();

    // поліморфізм через вказівник на базовий клас
    CircuitElement* basePtr = &copiedCapacitor;
    cout << "Polymorphic call via base pointer:" << endl;
    cout << "-> ";
    basePtr->PrintInfo();

    cout << "\n------------------------\n" << endl;

    // демонстрація контейнера та перевірки на зв'язність

    // зв'язний електричний ланцюг
    Circuit connectedCircuit;
    
    connectedCircuit.AddElement(new Resistor(1, 2, 100.0));
    connectedCircuit.AddElement(new Capacitor(2, 3, 0.005));
    connectedCircuit.AddElement(new Inductor(3, 4, 0.15));
    connectedCircuit.AddElement(new Resistor(4, 1, 50.0));

    cout << "- Connected circuit -" << endl;
    connectedCircuit.PrintAllElements();
    
    if (connectedCircuit.CheckConnectivity()) {
        cout << "-> Result: the circuit is connected." << endl;
    } else {
        cout << "-> Result: the circuit is disconnected." << endl;
    }

    cout << "\n------------------------\n" << endl;

    // незв'язний електричний ланцюг
    Circuit disconnectedCircuit;
    
    disconnectedCircuit.AddElement(new Resistor(1, 2, 220.0));
    disconnectedCircuit.AddElement(new Capacitor(3, 4, 0.001)); 

    cout << "- Disconnected circuit -" << endl;
    disconnectedCircuit.PrintAllElements();
    
    if (disconnectedCircuit.CheckConnectivity()) {
        cout << "-> Result: the circuit is connected." << endl;
    } else {
        cout << "-> Result: the circuit is disconnected." << endl;
    }

    return 0;
}