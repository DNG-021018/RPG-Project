## DEV LOG ##

Day 1: 28/8/2026
- Tạo Project
- Tìm assets
- Lên cho character system

Day 2: 29/8/2026
- Tạo base cho character
    + Base character được viết theo hướng components
    + Chia nhỏ các chức năng thành nhiều components khác nhau (health, combat, stamina, poise, movement, .....), character nào cần coponent nào thì có thể tự add
      * ví dụ:
        - giả sử các NPC thì ko cần health hay stamina hay poise mà chỉ cần movement và interact với nhân vật
        - hay các dummy thì chỉ cần animation để thể hiện hit reaction
        - nhân vật thì mỗi nhân vật một định nghĩa combat khác nhau như skills đặc biệt thì sẽ tự define riêng biệt trong chính class của nhân vật đó
- Setup animator cho nhân vật player

Day 3: 1/9/2026
- Thêm logic movement (move WASD, sprint, back step, roll, jump, fall, light fall, heavy fall)
- Thêm logic animation cho các movement
- Đang sử dụng logic để cho các animation -> đang cân nhắc đổi qua rootmotion để kết hợp với IK và giúp cho animation trở nên smooth và tạo cảm giác chân thân hơn trong tương lai
- Refactor lại base

Day 4: 
