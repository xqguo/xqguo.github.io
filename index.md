# 晓强空间

![](taiji.jpg)

个人兴趣爱好
* [Quantitaive Finance in Commodities](https://xqguo.github.io/CommodQuant/index.html)
* [书画历程](art.md)
* [口头禅](notes.md)
<div class="posts">
  <!-- {% for post in site.posts %}
    <article class="post">
      <h2><a href="{{ site.baseurl }}{{ post.url }}">{{ post.title}}</a></h2>
    </article>
  {% endfor %} -->
  {% for tag in site.tags %}
    <h3>{{ tag[0] }}</h3>
    <ul>
      {% for post in tag[1] %}
        <li><a href="{{ post.url }}">{{ post.title }}</a></li>
      {% endfor %}
    </ul>
  {% endfor %}
</div>