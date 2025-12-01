# Нормалізація бази даних

## Аналіз початкової схеми та Функціональні Залежності (ФЗ)

**Таблиця `MembershipPlan`**
* plan_id -> name
* plan_id -> access (Порушення 1NF: містить список значень, наприклад, 'gym_pool')
* plan_id -> duration_months
* plan_id -> price
* name -> plan_id

**Таблиця `Client`**
* client_id -> name, email, password, phone, created_at
* email -> client_id

**Таблиця `Coach`**
* coach_id -> name, specialization, email, password, created_at

**Таблиця `ClassType`**
* class_type_id -> name, description

**Таблиця `Class`**
* class_id -> class_type_id
* class_id -> coach_id
* class_id -> start_time, end_time
* class_id -> capacity
* class_id -> current_enrollment (Порушення: обчислюване поле, залежить від кількості записів в Enrollment)

**Таблиця `Enrollment`**
* enrollment_id -> client_id, class_id, registration_time
* {client_id, class_id} -> enrollment_id

**Таблиця `Invoice`**
* invoice_id -> client_id, amount, date, status, payment_method, notes

**Таблиця `Membership`**
* membership_id -> client_id
* membership_id -> plan_id
* membership_id -> start_date, end_date, is_active
* membership_id -> price (Транзитивна залежність: ціна залежить від плану)


## Процес Нормалізації

### Перша нормальна форма (1NF)

**Вимога:** Усі атрибути повинні бути атомарними (неподільними).

**Проблема:**
У таблиці `MembershipPlan` стовпець `access` містить значення, які логічно є списком. Це ускладнює пошук (наприклад, знайти всі плани з басейном).

**Рішення:**
1.  Створити таблицю `FacilityZone`.
2.  Створити таблицю зв'язку `PlanAccess` (many-to-Many між планами та зонами).
3.  Видалити стовпець `access` з `MembershipPlan`.

SQL зміни:
```sql
CREATE TABLE FacilityZone (
    zone_id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE PlanAccess (
    plan_id INTEGER NOT NULL,
    zone_id INTEGER NOT NULL,
    PRIMARY KEY (plan_id, zone_id),
    FOREIGN KEY (plan_id) REFERENCES MembershipPlan(plan_id) ON DELETE CASCADE,
    FOREIGN KEY (zone_id) REFERENCES FacilityZone(zone_id) ON DELETE CASCADE
);

ALTER TABLE MembershipPlan DROP COLUMN access;
```

### Друга нормальна форма (2NF)

**Вимога:** Таблиця знаходиться в 1NF, і всі неключові атрибути залежать від повного первинного ключа (відсутність часткових залежностей).

**Аналіз:** Більшість таблиць мають простий (сурогатний) первинний ключ (id), тому вони автоматично відповідають 2NF. Однак нова таблиця PlanAccess є зі складеним ключем (plan_id, zone_id). В ній немає неключових атрибутів, тому 2NF виконується.

**Висновок:** Схема відповідає вимогам 2NF.

### Третя нормальна форма (3NF)
**Вимога**: Таблиця знаходиться в 2NF, і відсутні транзитивні залежності (неключовий атрибут залежить від іншого неключового атрибута).

**Проблема 1 (транзитивна залежність)**: У таблиці `Membership` є поле `price`. Ланцюжок залежності: `membership_id -> plan_id -> price`. Ціна є атрибутом плану, а не конкретного абонемента (припускаючи фіксовану ціну плану). Зберігання її тут створює надлишковість.

**Проблема 2 (обчислювані дані)**: У таблиці `Class` є поле `current_enrollment`. Це значення дублює інформацію, яку можна отримати запитом `COUNT(*)` з таблиці `Enrollment`. Це може призвести до аномалій оновлення.

**Рішення**:

Видалити стовпець `price` з таблиці `Membership`.

Видалити стовпець `current_enrollment` з таблиці `Class` та перенести обчислення до бізнес-логіки.

SQL зміни:

```sql
ALTER TABLE Membership DROP COLUMN price;
ALTER TABLE Class DROP COLUMN current_enrollment;
```

### Результат
В результаті виконання лабораторної роботи базу даних було нормалізовано до 3NF:

* Усунено неатомарні атрибути (розділення зон доступу).
* Усунено транзитивні залежності (ціна абонемента).
* Видалено обчислювані поля для забезпечення цілісності даних.