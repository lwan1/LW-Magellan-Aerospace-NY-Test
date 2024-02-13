-- Create the Part database
CREATE DATABASE Part;

-- Connect to the Part database
\c Part;

-- Create the item table with the specified columns and constraints
CREATE TABLE item (
    id SERIAL PRIMARY KEY,
    item_name VARCHAR(50) NOT NULL,
    parent_item INTEGER REFERENCES item(id),
    cost INTEGER NOT NULL,
    req_date DATE NOT NULL
);

-- Insert data into the item table
INSERT INTO item (id, item_name, parent_item, cost, req_date) VALUES
    (1, 'Item1', null, 500, '2024-02-20'),
    (2, 'Sub1', 1, 200, '2024-02-10'),
    (3, 'Sub2', 1, 300, '2024-01-05'),
    (4, 'Sub3', 2, 300, '2024-01-02'),
    (5, 'Sub4', 2, 400, '2024-01-02'),
    (6, 'Item2', null, 600, '2024-03-15'),
    (7, 'Sub1', 6, 200, '2024-02-25');

-- Get_Total_Cost function
CREATE OR REPLACE FUNCTION Get_Total_Cost(item_name_param VARCHAR)
RETURNS INTEGER AS $$
DECLARE
    total_cost INTEGER;
BEGIN
    WITH RECURSIVE item_tree AS (
        SELECT id, cost
        FROM item
        WHERE item_name = item_name_param
        UNION
        SELECT i.id, i.cost
        FROM item i
        JOIN item_tree it ON i.parent_item = it.id
    )
    SELECT COALESCE(SUM(cost), 0) INTO total_cost
    FROM item_tree;

    RETURN total_cost;
END;
$$ LANGUAGE plpgsql;

-- Example usage of the Get_Total_Cost function
SELECT Get_Total_Cost('Sub1'); -- returns null
SELECT Get_Total_Cost('Item1'); -- returns 1700
