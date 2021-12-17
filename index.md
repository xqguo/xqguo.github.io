# 晓强空间

![](taiji.jpg)

个人兴趣爱好
* [Quantitaive Finance in Commodities](https://xqguo.github.io/CommodQuant/index.html)
* [书画历程](art.md)
* [口头禅](notes.md)
<div class="posts">
  {% for tag in site.tags %}
    <h3>{{ tag[0] }}</h3>
    <ul>
      {% for post in tag[1] %}
        <li><a href="{{ post.url }}">{{ post.date | date: "%Y-%m-%d" }} {{ post.title }}</a></li>
      {% endfor %}
    </ul>
  {% endfor %}
</div>
