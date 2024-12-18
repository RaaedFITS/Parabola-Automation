import sys
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.action_chains import ActionChains
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException, ElementClickInterceptedException
import time
import traceback


if len(sys.argv) < 3:
    print("ERROR: Provide the flow name and file path as arguments.")
    sys.exit(1)

flow_name = sys.argv[1]
file_path = sys.argv[2]
print(f"DEBUG: Flow name: {flow_name}, File path: {file_path}")

options = webdriver.ChromeOptions()
# Uncomment the next line for headless mode
#options.add_argument("--headless")
options.add_argument("--disable-gpu")
options.add_argument("--no-sandbox")
options.add_argument("--disable-dev-shm-usage")
options.add_argument("--window-size=1920,1080")

browser = webdriver.Chrome(options=options)

try:
    print("DEBUG: Navigating to Parabola login page")
    browser.get('https://parabola.io/app/flows/home')
    browser.implicitly_wait(10)

    print("DEBUG: Logging in")
    email_field = browser.find_element(By.XPATH, "//input[@placeholder='you@company.com']")
    password_field = browser.find_element(By.XPATH, "//input[@type='password']")
    login_button = browser.find_element(By.XPATH, "//button[contains(text(), 'Sign in')]")
    email_field.send_keys("itadmin@fitsexpress.com")
    password_field.send_keys("W1KI2G{8Oo$/|(")
    login_button.click()

    print("DEBUG: Waiting for dashboard")
    time.sleep(5)

    # Handle modal popups with payment error
    try:
        payment_modal_dismiss = WebDriverWait(browser, 10).until(
            EC.element_to_be_clickable((By.XPATH, "//div[@class='modal-dismiss css-5kqtmx']"))
        )
        payment_modal_dismiss.click()
        time.sleep(3)
        print("DEBUG: Closed payment error modal using ActionChains.")
    
    except TimeoutException:
        print("DEBUG: No payment error modal detected.")
        



    print(f"DEBUG: Looking for flow '{flow_name}'")
    flow_link = WebDriverWait(browser, 30).until(
        EC.element_to_be_clickable((By.XPATH, f"//a[contains(text(), '{flow_name}')]"))
    )
    flow_link.click()
    print(f"DEBUG: Flow '{flow_name}' opened")
      # Handle modal popups with payment error
    try:
        payment_modal_dismiss = WebDriverWait(browser, 10).until(
            EC.element_to_be_clickable((By.XPATH, "//div[@class='modal-dismiss css-5kqtmx']"))
        )
        payment_modal_dismiss.click()
        time.sleep(3)
        print("DEBUG: Closed payment error modal using ActionChains.")
    
    except TimeoutException:
        print("DEBUG: No payment error modal detected.")
        
    # Wait for the parent node that contains either "Pull from CSV file" or "Pull from Excel file"
    print("DEBUG: Locating the specific node (CSV or Excel)")
    parent_node = WebDriverWait(browser, 30).until(
        EC.presence_of_element_located((By.XPATH, 
            "//div[contains(@class,'react-flow__node-step') and "
            "((.//div[text()='Pull from CSV file']) or (.//div[text()='Pull from Excel file']))]"
        ))
    )

    # Once we have the parent node, locate the deepest clickable element within it
    print("DEBUG: Locating the deeper inner element to click within the node")
    deepest_element = parent_node.find_element(By.XPATH, ".//div[@class='css-1q4vvn7']")

    print("DEBUG: Performing a single click on the deeper element")
    actions = ActionChains(browser)
    actions.click(deepest_element).perform()
    time.sleep(1)
    print("DEBUG: Single click completed")

    print("DEBUG: Performing a double-click on the deeper element")
    actions.double_click(deepest_element).perform()
    print("DEBUG: Double-click completed")
    time.sleep(3)


    # Locate and upload file
    print("DEBUG: Locating the file upload input element")
    file_input = WebDriverWait(browser, 60).until(
        EC.presence_of_element_located((By.XPATH, "//div[contains(@class, 'css-1i2ya67')]//input[@type='file']"))
    )
    print(f"DEBUG: Uploading file: {file_path}")
    file_input.send_keys(file_path)
    print("DEBUG: File uploaded successfully")
    time.sleep(5)

    # Locate and click the Exit button
    print("DEBUG: Looking for the 'Exit' button")
    try:
        exit_button = WebDriverWait(browser, 10).until(
            EC.element_to_be_clickable((By.XPATH, "//div[contains(@class, 'modal-dismiss')]"))
        )
        exit_button.click()
        print("DEBUG: Clicked the 'Exit' button")
        time.sleep(2)
    except TimeoutException:
        print("WARNING: 'Exit' button not found or not clickable")

    # Wait for the first "Run Flow" button to become clickable
    print("DEBUG: Waiting for the first 'Run Flow' button to become clickable")
    first_run_flow_button = WebDriverWait(browser, 30).until(
        EC.element_to_be_clickable((By.XPATH, "//button[contains(@class, 'css-1eo098y') and contains(text(), 'Run Flow')]"))
    )
    print("DEBUG: The first 'Run Flow' button is now clickable")
    first_run_flow_button.click()
    print("DEBUG: Clicked on the first 'Run Flow' button")
    time.sleep(5)

    # Wait for the second "Run Flow" button to appear and click it
    print("DEBUG: Waiting for the second 'Run Flow' button to become visible")
    second_run_flow_button = WebDriverWait(browser, 30).until(
        EC.element_to_be_clickable((By.XPATH, "//div[@class='css-1aj1cwm el2um1b0']//div[contains(@class, 'css-1yzkv2u') and text()='Run Flow']"))
    )
    print("DEBUG: The second 'Run Flow' button is now clickable")
    second_run_flow_button.click()
    print("DEBUG: Clicked on the second 'Run Flow' button")
    time.sleep(5)

    # Wait for the "Run Flow" button to return to its initial state
    print("DEBUG: Waiting for the 'Run Flow' button to return to its initial state")
    WebDriverWait(browser, 30).until(
        EC.presence_of_element_located((By.XPATH, "//button[contains(@class, 'css-hgd1m') and contains(text(), 'Run Flow')]"))
    )
    print("DEBUG: The 'Run Flow' button has returned to its initial state")

except TimeoutException as te:
    print("ERROR: A timeout occurred while waiting for an element.")
    print(f"ERROR Details: {te}")
    traceback.print_exc()
except NoSuchElementException as nse:
    print("ERROR: An element could not be found on the page.")
    print(f"ERROR Details: {nse}")
    traceback.print_exc()
except WebDriverException as wde:
    print("ERROR: A WebDriver related error occurred.")
    print(f"ERROR Details: {wde}")
    traceback.print_exc()
except Exception as e:
    print("ERROR: An unexpected error occurred.")
    print(f"ERROR Details: {e}")
    traceback.print_exc()
finally:
    print("DEBUG: Closing the browser")
    browser.quit()
    print("Browser closed")
