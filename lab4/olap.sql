-- 1. БАЗОВА АГРЕГАЦІЯ

-- 1.1. Знайти загальну суму всіх оплачених інвойсів
SELECT SUM(amount) AS total_revenue
FROM Invoice
WHERE status = 'paid';

-- 1.2. Знайти середню вартість планів членства
SELECT AVG(price) AS average_plan_price
FROM MembershipPlan;

-- 1.3. Знайти мінімальну та максимальну місткість (capacity) серед усіх занять
SELECT MIN(capacity) AS min_capacity, MAX(capacity) AS max_capacity
FROM Class;

-- 1.4. Підрахувати загальну кількість клієнтів у базі
SELECT COUNT(*) AS total_clients
FROM Client;


-- 2. GROUP BY

-- 2.1. Підрахувати кількість інвойсів для кожного статусу
SELECT status, COUNT(*) AS invoice_count
FROM Invoice
GROUP BY status;

-- 2.2. Знайти кількість занять, які проводить кожен тренер
SELECT coach_id, COUNT(*) AS classes_count
FROM Class
GROUP BY coach_id;

-- 2.3. Порахувати загальну суму інвойсів за кожним методом оплати
SELECT payment_method, SUM(amount) AS total_amount
FROM Invoice
GROUP BY payment_method;


-- 3. HAVING

-- 3.1. Знайти тренерів, які проводять більше 5 занять
SELECT coach_id, COUNT(*) AS classes_count
FROM Class
GROUP BY coach_id
HAVING COUNT(*) > 5;

-- 3.2. Знайти типи абонементів (plan_id), які купили більше 100 разів
SELECT plan_id, COUNT(*) AS sales_count
FROM Membership
GROUP BY plan_id
HAVING COUNT(*) > 100;


-- 4. JOINs

-- 4.1. (INNER) JOIN: Отримати імена клієнтів та назви їхніх активних тарифних планів
SELECT 
    c.name AS client_name, 
    mp.name AS plan_name, 
    m.end_date
FROM Client c
JOIN Membership m ON c.client_id = m.client_id
JOIN MembershipPlan mp ON m.plan_id = mp.plan_id
WHERE m.is_active = TRUE;

-- 4.2. LEFT JOIN: Показати всіх тренерів та кількість їхніх занять (навіть якщо занять немає)
SELECT 
    co.name AS coach_name, 
    co.specialization,
    COUNT(cl.class_id) AS classes_scheduled
FROM Coach co
LEFT JOIN Class cl ON co.coach_id = cl.coach_id
GROUP BY co.name, co.specialization;

-- 4.3. CROSS JOIN: Створити комбінацію всіх клієнтів та всіх типів занять
SELECT 
    c.name AS client_name,
    ct.name AS class_type
FROM Client c
CROSS JOIN ClassType ct;

-- 4.4. MULTI-TABLE AGGREGATION: Показати тип заняття та кількість записаних на нього людей
SELECT 
    ct.name AS class_type_name,
    COUNT(e.enrollment_id) AS total_enrollments
FROM ClassType ct
JOIN Class cl ON ct.class_type_id = cl.class_type_id
JOIN Enrollment e ON cl.class_id = e.class_id
GROUP BY ct.name;


-- 5. ПІДЗАПИТИ

-- 5.1. Підзапит у WHERE: Знайти клієнтів, які заплатили за інвойсом суму, більшу за середню
SELECT name, email
FROM Client
WHERE client_id IN (
    SELECT client_id 
    FROM Invoice 
    WHERE amount > (SELECT AVG(amount) FROM Invoice)
);

-- 5.2. Підзапит у SELECT: Вивести ім'я клієнта та кількість його записів на заняття (як окремий стовпець)
SELECT 
    c.name,
    (SELECT COUNT(*) FROM Enrollment e WHERE e.client_id = c.client_id) AS enrollment_count
FROM Client c;

-- 5.3. Підзапит у HAVING: Показати типи занять, середня тривалість яких (тут умовно capacity) більша за середню по всіх заняттях
SELECT class_type_id, AVG(capacity) as avg_cap
FROM Class
GROUP BY class_type_id
HAVING AVG(capacity) > (SELECT AVG(capacity) FROM Class);