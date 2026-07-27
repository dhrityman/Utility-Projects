import requests
from bs4 import BeautifulSoup

def print_google_doc_grid(url):
    # 1. Fetch the HTML content from the published Google Doc URL
    try:
        response = requests.get(url)
        response.raise_for_status()
    except requests.exceptions.RequestException as e:
        print(f"Error fetching the document: {e}")
        return

    # 2. Parse the HTML using BeautifulSoup to find the table
    soup = BeautifulSoup(response.text, 'html.parser')
    table = soup.find('table')
    
    if not table:
        print("No data table found in the provided document.")
        return

    # 3. Extract the coordinates and characters from the table rows
    rows = table.find_all('tr')
    data_points = []
    
    # Track the positions of columns dynamically based on headers
    headers = [cell.get_text(strip=True) for cell in rows[0].find_all(['td', 'th'])]
    
    try:
        x_idx = headers.index('x-coordinate')
        char_idx = headers.index('Character')
        y_idx = headers.index('y-coordinate')
    except ValueError:
        # Fallback to standard 0, 1, 2 layout if headers are merged or strangely parsed
        x_idx, char_idx, y_idx = 0, 1, 2

    # Parse each data row
    for row in rows[1:]:
        cells = row.find_all(['td', 'th'])
        if len(cells) < 3:
            continue
        
        try:
            x = int(cells[x_idx].get_text(strip=True))
            char = cells[char_idx].get_text(strip=True)
            y = int(cells[y_idx].get_text(strip=True))
            
            # Standardise empty or space strings
            if not char:
                char = " "
                
            data_points.append((x, char, y))
        except (ValueError, IndexError):
            continue

    if not data_points:
        print("No valid character data could be parsed.")
        return

    # 4. Determine grid size (X and Y limits)
    max_x = max(point[0] for point in data_points)
    max_y = max(point[2] for point in data_points)

    # 5. Initialize an empty grid filled with spaces
    # Grid dimensions must account for 0-indexing (size = max_val + 1)
    grid = [[" " for _ in range(max_x + 1)] for _ in range(max_y + 1)]

    # 6. Populate the grid
    for x, char, y in data_points:
        grid[y][x] = char

    # 7. Print the grid top-to-bottom
    # In standard 2D text grids (like the 'F' example), 
    # y=0 is the top row, and y increases downwards.
    for row in grid:
        print("".join(row))

# Example Call:
doc_url = "https://docs.google.com/document/u/0/d/e/2PACX-1vTMOmshQe8YvaRXi6gEPKKlsC6UpFJSMAk4mQjLm_u1gmHdVVTaeh7nBNFBRlui0sTZ-snGwZM4DBCT/pub?pli=1"
print_google_doc_grid(doc_url)