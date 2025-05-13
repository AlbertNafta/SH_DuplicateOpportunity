# Mô tả và Hướng dẫn Sử Dụng Mã JavaScript Cho Command Button

debug cho code plugin C#: vô link của app D365 (phần grid - show danh sách) và sửa etn=plugintracelog
Ex: https://nhan-dev-env.crm5.dynamics.com/main.aspx?appid=7947952e-06da-ef11-8ee9-6045bd5b5666&pagetype=entitylist&etn=opportunity&viewid=00000000-0000-0000-00aa-000010003000&viewType=1039
-> https://nhan-dev-env.crm5.dynamics.com/main.aspx?appid=7947952e-06da-ef11-8ee9-6045bd5b5666&pagetype=entitylist&etn=plugintracelog&viewid=16f97046-9cc9-40de-914f-13da29b681ec&viewType=1039

## Mô tả:
File JavaScript này được tích hợp vào scripting của command button. Mục đích là gọi command button, sau đó command button này sẽ gọi plugin để sao chép cơ hội (duplicate opportunity).

## Vận hành:
(Chưa cập nhật thêm spinner và redirect)

## Cách hoạt động của mã JavaScript:

Mã JavaScript dưới đây thực hiện một số thao tác với đối tượng `Xrm.WebApi` trong môi trường Dynamics 365 (CRM) để tương tác với hệ thống.

### 1. Khởi tạo đối tượng và hàm:

Đoạn mã bắt đầu với việc khởi tạo một đối tượng `Example` nếu nó chưa tồn tại (sử dụng `window.Example || {}`).

- Hàm `formOnLoad` được định nghĩa, và hàm này sẽ được gọi khi một form trong CRM được tải.

### 2. Lấy thông tin về bản ghi hiện tại:

Trong hàm `formOnLoad`, đối tượng `formContext` đại diện cho ngữ cảnh của form đang mở.

- `entityId` được lấy từ `formContext.data.entity.getId()`, đây là ID của bản ghi hiện tại mà người dùng đang làm việc (ví dụ như một cơ hội [opportunity] trong hệ thống CRM).

- ID của bản ghi được in ra console với: `console.log("> Hello World", entityId)`.

### 3. Tạo yêu cầu gọi API:

Mã tạo một yêu cầu API gọi đến một hành động tùy chỉnh (custom action) có tên là `nhan_opportunity` (tạo trên Power App).

Đối tượng `execute_nhan_opportunity_Request` xác định các tham số của yêu cầu, bao gồm:

- **Tham số entity** chứa thông tin về bản ghi cơ hội (`opportunity`), với `entityType` là `"opportunity"` và `id` là `entityId`.
  
- Phương thức `getMetadata` trả về thông tin về hành động này, bao gồm các tham số và loại hành động.

- **Tham số operationName** là `"nhan_opportunity"`, chỉ ra tên hành động cần gọi.

### 4. Gọi API sử dụng Xrm.WebApi.execute:

Hàm `Xrm.WebApi.execute(execute_nhan_opportunity_Request)` được gọi để thực thi yêu cầu API.

- Nếu API trả về kết quả thành công (`response.ok`), nó sẽ trả về dữ liệu dưới dạng JSON.

### 5. Xử lý kết quả:

Dữ liệu nhận được từ API sẽ được xử lý trong phần `then` tiếp theo.

- `responseBody` là kết quả trả về từ API, và nó được lưu trong biến `result`.

- Kết quả này sẽ được in ra console, và tham số đầu ra của API (ví dụ, `output`) sẽ được lấy từ kết quả và in ra.

### 6. Bắt lỗi:

Nếu có lỗi trong quá trình gọi API, hàm `catch` sẽ bắt lỗi và in thông báo lỗi ra console.
